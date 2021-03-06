﻿// 
// Copyright (c) SoftSource Consulting, Inc. All rights reserved.
// Licensed under the MIT license.
// 
// https://github.com/SoftSourceConsulting/TelcoBot
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TelcoBot.Model;

namespace TelcoBot.Persistence
{
    public static class TelcoBotRepository
    {
        private static readonly ISimpleRepository<User> _users;
        private static readonly ISimpleRepository<UserPaymentMethod> _userPaymentMethods;
        private static readonly ISimpleRepository<InternetServiceLevel> _internetServiceLevels;
        private static readonly ISimpleRepository<Bill> _bills;

        static TelcoBotRepository()
        {
            string defaultDataPath = GetExecutingAssemblyFolder() + @"\Data\";

            _users = new SimpleXmlRepository<User, Items>(defaultDataPath, "UserData");
            _userPaymentMethods = new SimpleXmlRepository<UserPaymentMethod, Items>(defaultDataPath, "UserPaymentMethodData");
            _internetServiceLevels = new SimpleXmlRepository<InternetServiceLevel, Items>(defaultDataPath, "InternetServiceLevelData");
            _bills = new SimpleXmlRepository<Bill, Items>(defaultDataPath, "BillData");
        }

        #region users

        public static User FindUserByIdInChannel(string idInChannel) => _users.FirstOrDefault(u => u.IdInChannel == idInChannel);

        public static User FindUserByName(string firstName, string lastName)
        {
            User result = null;
            if (firstName != null || lastName != null)
            {
                if (firstName == null || lastName == null)
                {
                    // if we got just one name part, try to match it to either first or last, but accept the result only if its unique
                    string name = firstName ?? lastName;
                    IEnumerable<User> candidates = _users.Where(u => u.FirstName.Equals(name, StringComparison.CurrentCultureIgnoreCase) ||
                                                                u.LastName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    if (candidates.Count() == 1)
                        result = candidates.Single();
                }
                else
                {
                    result = _users.FirstOrDefault(u => u.FirstName.Equals(firstName, StringComparison.CurrentCultureIgnoreCase) &&
                                                   u.LastName.Equals(lastName, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            return result;
        }

        public static void SaveChanges(User user)
        {
            _users.Save(user);
        }

        #endregion users

        #region user payment methods

        public static IReadOnlyList<UserPaymentMethod> FindPaymentMethodsByUserId(int userId) => _userPaymentMethods.Where(u => u.UserId == userId).ToArray();

        public static UserPaymentMethod FindUserPaymentMethodById(int id) => _userPaymentMethods.FindById(id);

        #endregion user payment methods

        #region internet service levels

        public static IReadOnlyList<InternetServiceLevel> FindAllInternetServiceLevels() => _internetServiceLevels.ToArray();

        public static InternetServiceLevel FindInternetServiceLevelById(int id) => _internetServiceLevels.FindById(id);

        #endregion internet service levels

        #region bills

        public static IReadOnlyList<Bill> FindBillsByUser(User user)
        {
            // TODO: validate: user not null
            // maybe this should receive a userId, so nulls would not be an issue and we could just use a lambda here

            return _bills.Where(b => b.UserId == user.Id).OrderByDescending(b => b.Year).ThenByDescending(b => b.Month).ToArray();
        }

        public static Bill FindBillById(int id) => _bills.FindById(id);

        public static void SaveChanges(Bill bill) => _bills.Save(bill);


        #endregion bills

        private static string GetExecutingAssemblyFolder()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}