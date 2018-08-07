using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
        private readonly IConfigurationRoot _configuration;

        public PaymentService(SkillWarsContext context, ILoggerFactory loggerFactory, IConfigurationRoot configuration)
        {
            _logger = loggerFactory.CreateLogger<PaymentService>();
            _context = context;
            _configuration = configuration;
        }

        public async Task NewTransaction(List<KeyValuePair<string, StringValues>> query)
        {            
            _logger.LogDebug("New transaction");
            string _securityKey = _configuration.GetSection("Interkassa")["securityKey"];
            string _ik_co_id = _configuration.GetSection("Interkassa")["ik_co_id"];

            try
            {
                if (query.Where(u => u.Key == "ik_co_id").FirstOrDefault().Value != _ik_co_id)
                {
                    _logger.LogError("Transaction is not valid, step 1");
                    return;
                }
                else if (query.Where(u => u.Key == "ik_inv_st").FirstOrDefault().Value != "success")
                {
                    _logger.LogError("Transaction is not valid, step 2");
                    return;
                }

                //checking ik_sign
                var ik_sign = query.Where(k => k.Key == "ik_sign").FirstOrDefault();
                query.Remove(ik_sign);
                query.Sort(CompareByKey);

                string result = "";
                foreach (var kv in query)
                {
                    result += kv.Value + ":";
                }
                result += _securityKey;

                MD5 md5Hasher = MD5.Create();
                var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(result));
                if (ik_sign.Value != Convert.ToBase64String(data))
                {
                    _logger.LogError("Transaction is not valid, step 3");
                    return;
                }

                var transactionId = query.Where(k => k.Key == "ik_pm_no").FirstOrDefault().Value;
                var userId = query.Where(k => k.Key == "ik_x_userId").FirstOrDefault().Value;
                var amount = Convert.ToDouble(query.Where(k => k.Key == "ik_co_rfn").FirstOrDefault().Value);

                if(userId == "donate")
                {
                    //we have got donating 
                    _logger.LogError("We have got donating - " + amount);
                    return;
                }


                var user = _context.Users.Where(u => u.Id == Convert.ToInt32(userId)).FirstOrDefault();
                if (user != null)
                {
                    user.Balance += (int)amount;
                    await _context.SaveChangesAsync();
                    //send notification for that user
                }
                else
                {
                    //send message that we have got money from unknown 
                    _logger.LogError("We have got money from unknown user - " + amount);
                    return;
                }               

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while get new transaction - " + ex.Message);

            }
            return;
        }

        static int CompareByKey(KeyValuePair<string, StringValues> a, KeyValuePair<string, StringValues> b)
        {
            return a.Key.CompareTo(b.Key);
        }
    }
}
