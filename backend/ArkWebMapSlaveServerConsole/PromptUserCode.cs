using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ArkWebMapSlaveServerConsole
{
    partial class Program
    {
        /// <summary>
        /// A bit ugly. Asks the user for their status code until it works.
        /// </summary>
        static void IntroAndPromptUserForCode()
        {
            while (true)
            {
                Console.Clear();
                DrawWithColor("Hi!", ConsoleColor.Blue);
                DrawWithColor(" Welcome to the ArkWebMap companion program.\nThis program reads your Ark server save and runs the Ark web map using them.\n\nThe only map data sent over the internet is players and their tribes, if authenticated.\n");
                DrawWithColor("\nTo get started, type in your client setup code.", ConsoleColor.Blue);
                DrawWithColor(" You'll be able to set up this server from inside of your web browser.\nIf you don't have one, head over to https://ark.romanport.com/.\n\n");
                clientSessionId = Console.ReadLine();

                //Now that we have the code, check it.
                Console.Clear();
                DrawWithColor("Contacting master server...\n");
                try
                {
                    GetMasterMessages();
                    break;
                }
                catch (System.Net.WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            if ((int)response.StatusCode == 500)
                            {
                                //Likely that the user typed in the incorrect code. Tell them to check it
                                DrawWithColor("Invalid code. Please check the client setup code you entered.", ConsoleColor.Red);
                                Console.ReadLine();
                            }
                            else
                            {
                                DrawWithColor("Server unavailable. Please try again later.", ConsoleColor.Red);
                                Console.ReadLine();
                            }
                        }
                        else
                        {
                            DrawWithColor("Could not connect. Are you offline?", ConsoleColor.Red);
                            Console.ReadLine();
                        }
                    }
                    else
                    {
                        DrawWithColor("Could not connect. Are you offline?", ConsoleColor.Red);
                        Console.ReadLine();
                    }
                }
                catch
                {
                    DrawWithColor("Could not connect. Are you offline?", ConsoleColor.Red);
                    Console.ReadLine();
                }
            }

        }
    }
}
