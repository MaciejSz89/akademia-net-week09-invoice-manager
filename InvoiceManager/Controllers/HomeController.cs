using InvoiceManager.Models.Domains;
using InvoiceManager.Models.Repositories;
using InvoiceManager.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InvoiceManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private InvoiceRepository _invoiceRepository = new InvoiceRepository();
        private ClientRepository _clientRepository = new ClientRepository();
        private ProductRepository _productRepository = new ProductRepository();

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Invoices()
        {
            var userId = User.Identity.GetUserId();

            var invoices = _invoiceRepository.GetInvoices(userId);
            return View(invoices);
        }



        public ActionResult Clients()
        {
            var userId = User.Identity.GetUserId();

            var clients = _clientRepository.GetClients(userId);

            return View(clients);
        }



        public ActionResult Invoice(int id = 0)
        {
            var userId = User.Identity.GetUserId();
            var invoice = id == 0 ?
                            GetNewInvoice(userId) :
                            _invoiceRepository.GetInvoice(id, userId);
            var vm = PrepareInvoiceVm(invoice, userId);

            return View(vm);
        }

        public ActionResult Client(int id = 0)
        {
            var userId = User.Identity.GetUserId();
            ViewBag.Heading = id == 0 ? "Dodawanie nowego klienta" : "Klient";
            var client = id == 0 ?
                            GetNewClient(userId) :
                            _clientRepository.GetClient(id, userId);
            return View(client);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invoice(Invoice invoice)
        {
            var userId = User.Identity.GetUserId();
            invoice.UserId = userId;

            if (!ModelState.IsValid)
            {
                var vm = PrepareInvoiceVm(invoice, userId);
                return View("Invoice", vm);
            }

            if (invoice.Id == 0)
            {
                _invoiceRepository.Add(invoice);
            }
            else
            {
                _invoiceRepository.Update(invoice);
            };
            return RedirectToAction("Invoices");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Client(Client client)
        {
            var userId = User.Identity.GetUserId();
            client.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View("Client", client);
            }


            if (client.Id == 0)
            {
                _clientRepository.Add(client);
            }
            else
            {
                _clientRepository.Update(client);
            }
            return RedirectToAction("Clients");
        }



        [HttpPost]
        public ActionResult Address(Address address)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { id = 0 });
            }

            int addressId = 0;

            if (address.Id == 0)
            {
                addressId = _clientRepository.AddAddress(address).Id;
            }

            return Json(new { id = addressId });
        }

        private EditInvoiceViewModel PrepareInvoiceVm(Invoice invoice, string userId)
        {
            return new EditInvoiceViewModel
            {
                Invoice = invoice,
                Heading = invoice.Id == 0 ? "Dodawanie nowej faktury" : "Faktura",
                Clients = _clientRepository.GetClients(userId),
                MethodsOfPayment = _invoiceRepository.GetMethodsOfPayment()
            };
        }



        private Invoice GetNewInvoice(string userId)
        {
            return new Invoice
            {
                UserId = userId,
                CreatedDate = DateTime.Now,
                PaymentDate = DateTime.Now.AddDays(7)
            };
        }

        private Client GetNewClient(string userId)
        {
            ViewBag.Heading = "Dodawanie nowego klienta";

            return new Client
            {
                UserId = userId
            };
        }

        public ActionResult InvoicePosition(int invoiceId, int invoicePositionId = 0)
        {
            var userId = User.Identity.GetUserId();

            var invoicePosition = invoicePositionId == 0 ?
                                    GetNewPosition(invoiceId) :
                                    _invoiceRepository.GetInvoicePosition(invoicePositionId, userId);

            var vm = PrepareInvoicePositionVm(invoicePosition);


            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoicePosition(InvoicePosition invoicePosition)
        {
            var userId = User.Identity.GetUserId();

            var product = _productRepository
                            .GetProduct(invoicePosition.ProductId);

            if (!ModelState.IsValid)
            {
                var vm = PrepareInvoicePositionVm(invoicePosition);
                return View("InvoicePosition", vm);
            }

            invoicePosition.Value = invoicePosition.Quantity * product.Value;

            if (invoicePosition.Id == 0)
            {
                _invoiceRepository.AddPosition(invoicePosition, userId);
            }
            else
            {
                _invoiceRepository.UpdatePosition(invoicePosition, userId);
            }

            _invoiceRepository.UpdateInvoiceValue(invoicePosition.InvoiceId, userId);

            return RedirectToAction("Invoice", new { id = invoicePosition.InvoiceId });
        }

        [HttpPost]
        public ActionResult DeleteInvoice(int id)
        {

            try
            {
                var userId = User.Identity.GetUserId();
                _invoiceRepository.Delete(id, userId);
            }
            catch (Exception ex)
            {
                //logowanie
                return Json(new { Success = false, Message = ex.Message });
            }


            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult DeletePosition(int id, int invoiceId)
        {
            var invoiceValue = 0m;

            try
            {
                var userId = User.Identity.GetUserId();
                _invoiceRepository.DeletePosition(id, userId);
                invoiceValue = _invoiceRepository.UpdateInvoiceValue(invoiceId, userId);
            }
            catch (Exception ex)
            {
                //logowanie
                return Json(new { Success = false, Message = ex.Message });
            }


            return Json(new { Success = true, InvoiceValue = invoiceValue });
        }

        [HttpPost]
        public ActionResult DeleteClient(int id)
        {

            try
            {
                var userId = User.Identity.GetUserId();
                _clientRepository.Delete(id, userId);
            }
            catch (Exception ex)
            {
                //logowanie
                return Json(new { Success = false, Message = ex.Message });
            }


            return Json(new { Success = true });
        }

        private EditInvoicePositionViewModel PrepareInvoicePositionVm(InvoicePosition invoicePosition)
        {
            return new EditInvoicePositionViewModel
            {
                InvoicePosition = invoicePosition,
                Heading = invoicePosition.Id == 0 ? "Dodawanie nowej pozycji" : "Pozycja",
                Products = _productRepository.GetProducts()
            };
        }

        private InvoicePosition GetNewPosition(int invoiceId)
        {
            return new InvoicePosition
            {
                InvoiceId = invoiceId
            };
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";


            return View();
        }


        [AllowAnonymous]
        public ActionResult Contact()
        {

            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}