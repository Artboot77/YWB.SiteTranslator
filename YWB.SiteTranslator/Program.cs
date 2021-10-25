﻿using DeepL;
using System;
using System.Threading.Tasks;
using YWB.SiteTranslator.Helpers;

namespace YWB.SiteTranslator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Sites Translator by Yellow Web ver 2.5");
            Console.WriteLine("If you like this software, please, donate!");
            DonationHelper.Info();
            await Task.Delay(5000);
            Console.WriteLine();

            Console.WriteLine("What do you want to do?");
            Console.WriteLine("1. Extract website's text to csv");
            Console.WriteLine("2. Replace website's text from csv");
            var action = YesNoSelector.GetMenuAnswer(2);
            Console.Write("Enter your offer's name (as how it is written in the html):");
            var offerName = Console.ReadLine();
            switch (action)
            {
                case 1:
                {
                    var ex = new HtmlProcessor();
                    var txt = await ex.ExtractTextAsync(offerName);
                    var answer = YesNoSelector.ReadAnswerEqualsYes("Do you want to translate extracted text using DeepL API?");
                    if (answer)
                    {
                        Console.WriteLine("To which language do you want to translate?");
                        Console.WriteLine("1.English");
                        Console.WriteLine("2.Russian");
                        Console.WriteLine("3.Custom");
                        var l = YesNoSelector.GetMenuAnswer(3);

                        if (l == 3) Console.Write("Enter language name:");

                        var language = l switch
                        {
                            2 => Language.Russian,
                            3 => Enum.Parse<Language>(Console.ReadLine()),
                            _ => Language.English
                        };

                        var newOfferName = offerName;
                        answer = YesNoSelector.ReadAnswerEqualsYes("Do you want to change the offer in the autotranslated text?");
                        if (answer)
                        {
                            Console.Write("Enter new offer name:");
                            newOfferName = Console.ReadLine();
                        }
                        var trans = new DeeplService();
                        foreach (var ti in txt)
                        {
                            if (ti.Text.Length < 2) continue;
                            var tt = ti.Text;
                            if (!string.IsNullOrEmpty(offerName) && !string.IsNullOrEmpty(newOfferName))
                                tt = tt.Replace(offerName, newOfferName);
                            ti.Translation = await trans.TranslateAsync(tt, language);
                        }
                        Console.WriteLine("Translation complete!");
                        answer = YesNoSelector.ReadAnswerEqualsYes("Do you want to translate the website's html with the autotranslated text?");
                        if (answer)
                        {
                            var newName = await ex.TranslateAsync(offerName, newOfferName, txt);
                            Console.WriteLine($@"Tranlation saved to ""{newName}"" file in the website's directory.");
                        }

                    }
                    var csv = new CSVProcessor();
                    csv.Write(txt);
                    Console.WriteLine("Text extracted to \"translation.csv\" file in the program's directory.");
                    break;
                }
                case 2:
                {
                    Console.Write("Enter translated offer's name (or Enter if the same):");
                    var newOfferName = Console.ReadLine();
                    if (string.IsNullOrEmpty(newOfferName)) newOfferName = offerName;
                    var csv = new CSVProcessor();
                    var txt = csv.Read();
                    var ex = new HtmlProcessor();
                    var newName = await ex.TranslateAsync(offerName, newOfferName, txt);
                    Console.WriteLine($@"Tranlation saved to ""{newName}"" file in the website's directory.");
                    break;
                }
            }



            Console.WriteLine("All done. Press any key to exit... and don't forget to donate!");
            DonationHelper.Info();
            Console.ReadKey();
        }
    }
}
