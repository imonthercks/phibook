using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using FileHelpers;
using JsonFx.Json;
using Nancy.Cryptography;
using RestSharp;

namespace PhiBook.BatchLoader
{
    class Program
    {
        public static SecureString GetPassword()
        {
            var pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    pwd.RemoveAt(pwd.Length - 1);
                    Console.Write("\b \b");
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }

        static void Main(string[] args)
        {

            Console.WriteLine("Enter UserName: ");
            var userName = Console.ReadLine();
            Console.WriteLine("Enter Password:");
            var password = GetPassword();
            string importFile = string.Empty;
            string postUrl = string.Empty;
            string source = string.Empty;

            if (args.Length >= 1)
                importFile = args[0];

            if (args.Length >= 2)
                postUrl = args[1];

            if (args.Length >= 3)
                source = args[2];

            var engine = new FileHelperEngine<Contact>();

            if (!File.Exists(importFile))
                return;

            var tmpImportFile = importFile + ".imp";
            using (var file = new StreamReader(importFile)){
                using (var writer = new StreamWriter(tmpImportFile))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {

                        writer.WriteLine(line + (!line.Trim().EndsWith(",") ? "," : ""));
                    }
                }
                file.Close();
            }

            engine.BeforeReadRecord += EngineBeforeReadRecord;
            engine.AfterReadRecord += EngineAfterReadRecord;

            var list = engine.ReadFile(tmpImportFile, -1);

            var client = new RestClient {BaseUrl = postUrl, Authenticator = new PhiBookAuthenticator(postUrl, userName, password.ToString())};

            foreach (var contact in list)
            {

                var request = new RestRequest(Method.GET) { Resource = "api/Contact/{id}" };
                request.AddUrlSegment("id", contact.Id);


                var getResponse = client.Execute<dynamic>(request);
                dynamic responseContact = new JsonReader().Read(getResponse.Content);

                var post = new RestRequest(Method.POST) { Resource = "api/Contact", RequestFormat = DataFormat.Json };
                var addresses = new ArrayList();


                if (!string.IsNullOrEmpty(contact.Address1))
                {
                    var addressMatch = false;
                    if (responseContact != null && responseContact.AllAddresses != null)
                    {
                        addresses.AddRange(responseContact.AllAddresses);
                        foreach (var address in responseContact.AllAddresses)
                        {
                            if (address.Address1 == contact.Address1 &&
                                address.Address2 == contact.Address2 &&
                                address.City == contact.City &&
                                address.State == contact.State &&
                                address.PostalCode == contact.PostalCode)
                            {
                                addressMatch = true;
                            }
                        }
                    }
                    if (!addressMatch)
                        addresses.Add(new
                                          {
                                              contact.Address1, contact.Address2, contact.City, contact.State, contact.PostalCode, contact.Country, Updated = contact.Updated ?? DateTime.MinValue, Source = source
                                          }
                            );

                }

                ArrayList emailAddresses = AddEmailAddresses(source, responseContact, contact.Email, contact.Updated);
                emailAddresses.AddRange(AddEmailAddresses(source, responseContact, contact.Email2, contact.Updated));

                ArrayList phoneNumbers = AddPhoneNumbers(source, responseContact, contact.HomePhone, "Home", contact.Updated);
                phoneNumbers.AddRange(AddPhoneNumbers(source, responseContact, contact.MobilePhone, "Mobile", contact.Updated));
                phoneNumbers.AddRange(AddPhoneNumbers(source, responseContact, contact.WorkPhone, "Work", contact.Updated));

                dynamic newContact = new
                                         {
                                             Id = "contact/" + contact.Id,
                                             Status = TryGetValue<string>(responseContact, "Status") ?? contact.Status,
                                             FirstName = TryGetValue<string>(responseContact, "FirstName") ?? contact.FirstName,
                                             MiddleInitial = TryGetValue<string>(responseContact, "MiddleInitial") ?? contact.MiddleInitial,
                                             LastName = TryGetValue<string>(responseContact, "LastName") ?? contact.LastName,
                                             InitiationDate = TryGetDateValue(responseContact, "InitationDate") ?? contact.InitiationDate == DateTime.MinValue ? null : contact.InitiationDate,
                                             DateOfDeath = TryGetDateValue(responseContact, "DateOfDeath") ?? contact.DateOfDeath == DateTime.MinValue ? null : contact.DateOfDeath,
                                             ConfirmedMailingAddress =
                                                 GetConfirmedMailingAddress(source, contact, responseContact),
                                             AllAddresses = addresses.ToArray(typeof (object)),
                                             AllEmailAddress = emailAddresses.ToArray(typeof (object)),
                                             AllPhoneNumbers = phoneNumbers.ToArray(typeof (object))
                                         };


                post.AddBody(newContact);

                var postResponse = client.Execute(post);

                if (postResponse.ResponseStatus != ResponseStatus.Completed)
                    break;
            }

            Console.ReadKey();
        }

        static void EngineBeforeReadRecord(EngineBase engine, BeforeReadRecordEventArgs<Contact> e)
        {
            Console.WriteLine(e.RecordLine);
        }

        static void EngineAfterReadRecord(EngineBase engine, AfterReadRecordEventArgs<Contact> e)
        {
            Console.WriteLine(e.Record.Id);
        }

        private static ArrayList AddEmailAddresses(string source, dynamic responseContact, string newEmail, DateTime? updated)
        {
            var emailAddresses = new ArrayList();
            if (!string.IsNullOrEmpty(newEmail))
            {
                var emailMatch = false;
                if (responseContact != null && responseContact.AllEmailAddresses != null)
                {
                    emailAddresses.AddRange(responseContact.AllEmailAddresses);
                    foreach (var email in responseContact.AllEmailAddresses)
                    {
                        if (email.Email == newEmail)
                        {
                            emailMatch = true;
                        }
                    }
                }
                if (!emailMatch)
                    emailAddresses.Add(new
                                           {
                                               Email = newEmail,
                                               Updated = updated ?? DateTime.MinValue,
                                               Source = source
                                           }
                        );
            }
            return emailAddresses;
        }
        private static string CleanPhone(string phone)
        {
            var digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(phone, "");
        }

        private static ArrayList AddPhoneNumbers(string source, dynamic responseContact, string newPhone, string newPhoneType, DateTime? updated)
        {
            var phoneNumbers = new ArrayList();


            var strippedPhone = CleanPhone(newPhone);

            if (!string.IsNullOrEmpty(newPhone))
            {
                var phoneMatch = false;
                if (responseContact != null && responseContact.AllPhoneNumbers != null)
                {
                    phoneNumbers.AddRange(responseContact.AllPhoneNumbers);
                    foreach (var phone in responseContact.AllPhoneNumbers)
                    {
                        if (phone.Phone == strippedPhone)
                        {
                            phoneMatch = true;
                        }
                    }
                }
                if (!phoneMatch)
                    phoneNumbers.Add(new
                    {
                        Phone = strippedPhone,
                        PhoneType = newPhoneType,
                        Updated = updated ?? DateTime.MinValue,
                        Source = source
                    }
                        );
            }
            return phoneNumbers;
        }

        private static dynamic GetConfirmedMailingAddress(string source, Contact contact, dynamic responseContact)
        {
            var existingAddress = TryGetValue<ExpandoObject>(responseContact, "ConfirmedMailingAddress");

            if (existingAddress != null)
                return existingAddress;

            return contact.BadAddress == "No"
                       ? new
                             {
                                 contact.Address1,
                                 contact.Address2,
                                 contact.City,
                                 contact.State,
                                 contact.PostalCode,
                                 contact.Country,
                                 Updated = contact.Updated ?? DateTime.MinValue,
                                 Source = source
                             }
                       : null;
        }

        private static T TryGetValue<T>(IDictionary<string, object> obj, string propertyName) where T : class
        {
            if (obj == null)
                return null;

            if (obj.ContainsKey(propertyName))
                return obj[propertyName] as T;

            return null;
        }

        private static DateTime? TryGetDateValue(IDictionary<string, object> obj, string propertyName)
        {
            if (obj == null)
                return null;

            if (obj.ContainsKey(propertyName))
                return obj[propertyName] as DateTime?;

            return null;
        }

        /// <summary>
        /// Encrypt and sign the cookie contents
        /// </summary>
        /// <param name="cookieValue">Plain text cookie value</param>
        /// <returns>Encrypted and signed string</returns>
        private static string EncryptAndSignCookie(string cookieValue)
        {
            var encryptedCookie = CryptographyConfiguration.Default.EncryptionProvider.Encrypt(cookieValue);
            var hmacBytes = GenerateHmac(encryptedCookie);
            var hmacString = Convert.ToBase64String(hmacBytes);

            return String.Format("{1}{0}", encryptedCookie, hmacString);
        }

        /// <summary>
        /// Generate a hmac for the encrypted cookie string
        /// </summary>
        /// <param name="encryptedCookie">Encrypted cookie string</param>
        /// <returns>Hmac byte array</returns>
        private static byte[] GenerateHmac(string encryptedCookie)
        {
            return CryptographyConfiguration.Default.HmacProvider.GenerateHmac(encryptedCookie);
        }


    }
}
