﻿using StockDataServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StockDataServer.Controllers
{
    public class AccountController : ApiController
    {
        [HttpGet]
        public bool Login(string username, string password)
        {
            DBStockTrainerDataContext db = new DBStockTrainerDataContext();
            string match = (from a in db.GetTable<Account>()
                         where (a.Username == username)
                         select a.Password).SingleOrDefault();

            if (password == match && !string.IsNullOrWhiteSpace(password))
            {
                return true;
            }

            return false;
        }

        [HttpGet]
        public AccountTabModel GetAccountTabDataByUsername(string username)
        {
            DBStockTrainerDataContext db = new DBStockTrainerDataContext();

            var matchedPortfolios = (from p in db.GetTable<Portfolio>()
                                     join s in db.GetTable<Stock>() on p.Ticker equals s.Ticker
                                     where p.Username == username
                                     select new { p, s }).ToList();
            decimal stocksValue = 0;
            foreach (var item in matchedPortfolios)
            {
                stocksValue += item.s.Price * item.p.NumStocks;
            }

            return (from a in db.GetTable<Account>()
                    where (a.Username == username)
                    select new AccountTabModel {
                        Username = a.Username,
                        StartingInvestment = a.StartingInvestment,
                        StocksValue = stocksValue,
                        AvailableCash = a.AvailableCash,
                        TotalValue = stocksValue + a.AvailableCash,
                        Position = (stocksValue + a.AvailableCash) - a.StartingInvestment,
                        TotalTrans = a.TotalTrans,
                        PositiveTrans = a.PositiveTrans,
                        NegativeTrans = a.NegativeTrans
                    }).SingleOrDefault();
        }

        [HttpPost]
        public bool SignUp(SignUpModel signUpInfo)
        {
            DBStockTrainerDataContext db = new DBStockTrainerDataContext();

            var matchedUser = (from a in db.GetTable<Account>()
                               where a.Username == signUpInfo.Username
                               select a).SingleOrDefault();

            if (matchedUser == null)
            {
                try
                {
                    Account account = new Account();
                    account.Username = signUpInfo.Username;
                    account.Password = signUpInfo.Password;
                    account.Fullname = signUpInfo.FullName;
                    account.FirstSecurityQuestion = signUpInfo.FirstSecurityQuestion;
                    account.FirstSecurityAnswer = signUpInfo.FirstSecurityAnswer;
                    account.SecondSecurityQuestion = signUpInfo.SecondSecurityQuestion;
                    account.SecondSecurityAnswer = signUpInfo.SecondSecurityAnswer;
                    account.StartingInvestment = 20000;
                    account.AvailableCash = 20000;
                    account.TotalTrans = 0;
                    account.PositiveTrans = 0;
                    account.NegativeTrans = 0;

                    db.Accounts.InsertOnSubmit(account);
                    db.SubmitChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
    }
}
