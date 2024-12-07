using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        while (true) // حلقة لا نهائية لإعادة التشغيل عند حدوث خطأ
        {
            try
            {
                // إعداد Selenium
                var options = new ChromeOptions();
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");

                string loginUrl = "http://ugeen.live/signin.html"; // رابط تسجيل الدخول
                string username = "mohammed2024@gmail.com"; // اسم المستخدم
                string password = "M12344m@"; // كلمة المرور
                string apiKey = "6LexPvMgAAAAALN68SVJjCdXthMxNSs9Sp6Q4Pdr"; // ضع هنا مفتاح 2Captcha الخاص بك

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // فتح صفحة تسجيل الدخول
                    driver.Navigate().GoToUrl(loginUrl);

                    // إدخال بيانات تسجيل الدخول
                    driver.FindElement(By.Id("email")).SendKeys(username);
                    driver.FindElement(By.Id("password")).SendKeys(password);

                    Thread.Sleep(3000); // الانتظار قليلاً بعد إدخال البيانات

                    // النقر على زر تسجيل الدخول
                    driver.FindElement(By.Id("submit")).Click();

                    // انتظار تحميل الصفحة
                    Thread.Sleep(3000);

                    Thread.Sleep(3000); // الانتظار قليلاً بعد النقر على زر إرسال

                    // فتح صفحة "renew.html"
                    driver.Navigate().GoToUrl("http://ugeen.live/renew.html");
                    Thread.Sleep(3000); // الانتظار قليلاً بعد تحميل الصفحة

                    // النقر على رابط "تحميل كود التفعيل"
                    driver.FindElement(By.CssSelector("a[href='renew.html']")).Click();
                    Thread.Sleep(2000); // الانتظار قليلاً بعد النقر على الرابط

                    // النقر على الزر "طلب الرابط"
                    driver.FindElement(By.CssSelector(".sp-plan-btn .btn-primary.request-code")).Click();
                    Thread.Sleep(2000); // الانتظار قليلاً بعد النقر على الزر

                    // استخراج الـ Token من localStorage
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    string token = (string)jsExecutor.ExecuteScript("return window.localStorage.getItem('downloadToken');");
                    Console.WriteLine("Token: " + token);

                    var handler = new JwtSecurityTokenHandler();
                    string key = "";

                    // التحقق من أن التوكن صالح
                    if (handler.CanReadToken(token))
                    {
                        // فك التوكن
                        var jwtToken = handler.ReadJwtToken(token);

                        // استخراج الـ Payload (البيانات)
                        var claims = jwtToken.Payload;

                        // طباعة جميع الـ Claims
                        Console.WriteLine("Payload Claims:");
                        foreach (var claim in claims)
                        {
                            Console.WriteLine($"{claim.Key}: {claim.Value}");
                        }

                        // استخراج قيمة "code.code" من الـ Payload
                        if (claims.ContainsKey("code"))
                        {
                            // تحويل قيمة code إلى كائن JSON
                            var codeJson = claims["code"]?.ToString();
                            if (!string.IsNullOrEmpty(codeJson))
                            {
                                // فك JSON لاستخراج "code"
                                var codeObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(codeJson);
                                if (codeObject != null && codeObject.ContainsKey("code"))
                                {
                                    var codeValue = codeObject["code"];
                                    Console.WriteLine("code.code: " + codeValue);
                                    key = codeValue.ToString();
                                }
                                else
                                {
                                    Console.WriteLine("code.code غير موجود في التوكن.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("القيمة المرتبطة بـ 'code' غير صالحة أو فارغة.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("key 'code' غير موجود في التوكن.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("التوكن غير صالح.");
                    }
                    Thread.Sleep(1000); // الانتظار قليلاً بعد إدخال التوكن

                    // إدخال التوكن في حقل النص
                    IWebElement codeInput = driver.FindElement(By.Id("code-input"));
                    codeInput.Clear(); // التأكد من أنه لا يوجد قيمة سابقة
                    codeInput.SendKeys(key); // إدخال التوكن
                    Thread.Sleep(1000); // الانتظار قليلاً بعد إدخال التوكن

                    // اختيار الحزمة المناسبة
                    driver.FindElement(By.CssSelector("input#pack-plan-384")).Click();
                    Thread.Sleep(1000); // الانتظار قليلاً بعد اختيار الحزمة

                    // النقر على زر "تجديد الإشتراك الآن"
                    driver.FindElement(By.CssSelector(".btn.btn-primary.btn-xl.btn-block.rounded-0.py-3.submit")).Click();
                    Thread.Sleep(5000); // الانتظار قليلاً بعد النقر على الزر
                }

                // إذا انتهت العملية بنجاح، يتم إنهاء الحلقة
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("حدث خطأ أثناء التنفيذ: " + ex.Message);
                Console.WriteLine("إعادة المحاولة...");
                Thread.Sleep(2000); // الانتظار قليلاً قبل إعادة المحاولة
            }
        }
    }
}
