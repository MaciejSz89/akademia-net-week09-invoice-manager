using InvoiceManager.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

namespace InvoiceManager.Models.Repositories
{
    public class ClientRepository
    {
        public List<Client> GetClients(string userId)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Clients
                              .Include(x => x.Address)
                              .Where(x => x.UserId == userId)
                              .ToList();
            }
        }

        public Client GetClient(int id, string userId)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Clients
                              .Include(x => x.Address)
                              .Where(x => x.UserId == userId && x.Id == id)
                              .Single();
            }
        }

        public void Add(Client client)
        {
            using (var context = new ApplicationDbContext())
            {
                client.AddressId = AddAddress(client.Address).Id;
                client.Address = null;
                context.Clients.Add(client);
                context.SaveChanges();
            }
        }

        public void Update(Client client)
        {
            using (var context = new ApplicationDbContext())
            {
                
                var clientToUpdate = context.Clients
                                             .Single(x => x.Id == client.Id 
                                                       && x.UserId == client.UserId);

                clientToUpdate.Name = client.Name;
                clientToUpdate.AddressId = UpdateAddress(client.Address).Id;
                clientToUpdate.Email = client.Email;

                context.SaveChanges();
            }
        }


        public Address AddAddress(Address address)
        {
            using (var context = new ApplicationDbContext())
            {
                var matchingAddress = context.Addresses.Where(x => x.Street == address.Street
                                                              && x.Number == address.Number
                                                              && x.City == address.City
                                                              && x.PostalCode == address.PostalCode)
                                                     .FirstOrDefault();
                if (matchingAddress == null)
                {
                    address.Id = 0;
                    context.Addresses.Add(address);
                    context.SaveChanges();
                    address.Id = context.Addresses.Where(x => x.Street == address.Street
                                                               && x.Number == address.Number
                                                               && x.City == address.City
                                                               && x.PostalCode == address.PostalCode)
                                                     .FirstOrDefault()
                                                     .Id;
                }
                else
                {
                    address.Id = matchingAddress.Id;
                }
                return address;
            }
        }

        public Address UpdateAddress(Address address)
        {
            using (var context = new ApplicationDbContext())
            {
                var addressToUpdate = context.Addresses
                                             .Where(x => x.Id == address.Id)
                                             .Single();
                var addressChanged = addressToUpdate.Street != address.Street
                                  || addressToUpdate.Number != address.Number
                                  || addressToUpdate.City != address.City
                                  || addressToUpdate.PostalCode != address.PostalCode;

                if (addressChanged)
                {
                    
                    AddAddress(address);
                    address = context.Addresses.Where(x => x.Street == address.Street
                                                        && x.Number == address.Number
                                                        && x.City == address.City
                                                        && x.PostalCode == address.PostalCode)
                                               .Single();
                }

                return address;

            }
        }



        public void Delete(int id, string userId)
        {

            using (var context = new ApplicationDbContext())
            {
                var clientToDelete = context.Clients
                                             .Single(x =>
                                                         x.Id == id &&
                                                         x.UserId == userId);
                context.Clients.Remove(clientToDelete);
                context.SaveChanges();

            }
        }

    }
}