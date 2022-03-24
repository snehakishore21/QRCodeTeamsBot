// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Newtonsoft.Json;
using System.Drawing;
using QRCodeDecoderLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZXing;


namespace QRCodeTeamsBot.Bots
{
    // Represents a bot that processes incoming activities.
    // For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    // This is a Transient lifetime service. Transient lifetime services are created
    // each time they're requested. For each Activity received, a new instance of this
    // class is created. Objects that are expensive to construct, or have a lifetime
    // beyond the single turn, should be carefully managed.

    public class AttachmentsBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        private static DecodeQR decodeQR = new DecodeQR();

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = await ProcessInput(turnContext, cancellationToken);
            // Respond to the user.
            // await turnContext.SendActivityAsync(reply, cancellationToken);
            if (reply.Text != "Thanks for your time.")
            {
                var card0 = new HeroCard
                {
                    Title = $"QR Code(\'{QRName}\') detected and analyzed.",
                    Subtitle = "Please find details about the QR Code below:",
                    Text = reply.Text,

                };
                var reply0 = MessageFactory.Attachment(card0.ToAttachment());
                await turnContext.SendActivityAsync(reply0, cancellationToken);

                var card = new HeroCard
                {
                    Text = "Do you want to continue opening this QR Code URL/Action?",
                    Buttons = new List<CardAction>
                {
                    // Note that some channels require different values to be used in order to get buttons to display text.
                    // In this code the emulator is accounted for with the 'title' parameter, but in other channels you may
                    // need to provide a value for other parameters like 'text' or 'displayText'.
                    new CardAction(ActionTypes.OpenUrl, title: "1. Yes", value: $"{URL}"),
                    new CardAction(ActionTypes.ImBack, title: "2. No", value: "No"),
                },
                };

                var reply2 = MessageFactory.Attachment(card.ToAttachment());
                await turnContext.SendActivityAsync(reply2, cancellationToken);
            }
            else
            {
                var card0 = new HeroCard
                { 
                    Text = reply.Text,
                };
                var reply0 = MessageFactory.Attachment(card0.ToAttachment());
                await turnContext.SendActivityAsync(reply0, cancellationToken);
            }
            await DisplayOptionsAsync(turnContext, cancellationToken);
        }

        private static async Task DisplayOptionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create a HeroCard with options for the user to interact with the bot.
            var card = new HeroCard
            {
                Text = "You can upload an QR Code to check whether its malicious or not.",
            };

            var reply = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // Greet the user and give them instructions on how to interact with the bot.
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {

                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var card0 = new HeroCard
                    {
                        Title = $"Welcome to Malicious QR Code Detector Bot.",
                        Subtitle = "This bot will inform you to whether your QR Code is malicious or not.\r\n Upload QR code and test.",

                    };
                    var reply0 = MessageFactory.Attachment(card0.ToAttachment());

                    await turnContext.SendActivityAsync(reply0,
                        cancellationToken: cancellationToken);
                    await DisplayOptionsAsync(turnContext, cancellationToken);
                }
            }
        }

        // Given the input from the message, create the response.
        private static async Task<IMessageActivity> ProcessInput(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            IMessageActivity reply = null;
            IMessageActivity reply2 = null;

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                // We know the user is sending an attachment as there is at least one item
                // in the Attachments list.
                try
                {
                    reply = HandleIncomingAttachment(activity);
                }
                catch
                {
                    reply = MessageFactory.Text("Something went wrong in processing input. Please try again.");
                }
                var card = new HeroCard
                {
                    Subtitle = "You can upload an QR Code to check whether its malicious or not.",
                };
            }
            else
            {
                // Send at attachment to the user.
                reply2 = await HandleOutgoingAttachment(turnContext, activity, cancellationToken);
                return reply2;
            }

            return reply;
        }

        // Returns a reply with the requested Attachment
        private static async Task<IMessageActivity> HandleOutgoingAttachment(ITurnContext turnContext, IMessageActivity activity, CancellationToken cancellationToken)
        {
            // Look at the user input, and figure out what kind of attachment to send.
            IMessageActivity reply = null;
            if (activity.Text.StartsWith("1"))
            {
                reply = MessageFactory.Text("This is an inline attachment.");
            }
            else if (activity.Text.StartsWith("2"))
            {
                reply = MessageFactory.Text("This is an attachment from a HTTP URL.");
            }
            else if (activity.Text.StartsWith("No"))
            {
                reply = MessageFactory.Text("Thanks for your time.");
            }
            else
            {
                // The user did not enter input that this bot was built to handle.
                reply = MessageFactory.Text("Your input was not recognized please try again.");
            }

            return reply;
        }

        public static string result = "";

        public class QRCode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IFormFile imageFile { get; set; }
        }

        public class UploadedFile
        {
            public string FileFullName { get; set; }
            public byte[] Data { get; set; }

            public UploadedFile(string filePath)
            {
                FileFullName = Path.GetFileName(Normalize(filePath));
                Data = File.ReadAllBytes(filePath);
            }

            private string Normalize(string input)
            {
                return new string(input
                        .Normalize(System.Text.NormalizationForm.FormD)
                        .Replace(" ", string.Empty)
                        .ToCharArray()
                        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        .ToArray());
            }
        }

       
        private static string URL = "URL Not Detected";
        private static string Details = "NA";
        private static string ThreatType = "NA";
        private static string maliciousnesslevel = "NA";
        private static string maliciousString = "NA";
        private static bool IsMalicious = true;
        private static string FilePath = "";
        private static string QRName = "";
        private static string websiteLink ="Website Not Detected";
        private static bool isPaymentLink = false;
        private static string merchantName = "NA";


        private static Bitmap bitmap ;

        // Handle attachments uploaded by users. The bot receives an <see cref="Attachment"/> in an <see cref="Activity"/>.
        // The activity has a "IList{T}" of attachments.    
        // Not all channels allow users to upload files. Some channels have restrictions
        // on file type, size, and other attributes. Consult the documentation for the channel for
        // more information. For example Skype's limits are here
        // <see ref="https://support.skype.com/en/faq/FA34644/skype-file-sharing-file-types-size-and-time-limits"/>.
        private static IMessageActivity HandleIncomingAttachment(IMessageActivity activity)
        {
            var replyText = string.Empty;
            HashSet<string> results = new HashSet<string>();
            try
            {
                foreach (var file in activity.Attachments)
                {
                    // Determine where the file is hosted.
                    var remoteFileUrl = file.ContentUrl;
                    URL = "URL Not Detected";
                    Details = "NA";
                    ThreatType = "NA";
                    maliciousnesslevel = "NA";
                    IsMalicious = true;
                    QRName = file.Name;

                    // Save the attachment to the system temp directory.
                    var localFileName = Path.Combine(Path.GetTempPath(), file.Name);
                    maliciousString = "";

                    FilePath = localFileName;
                    try
                    { // Download the actual attachment
                        using (var webClient = new WebClient())
                        {
                            webClient.DownloadFile(remoteFileUrl, localFileName);
                        }

                        FormFile QRFile;
                        using (var stream = System.IO.File.OpenRead(localFileName))
                        {
                            QRFile = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(stream.Name));
                        }
                        QRCode qr = new QRCode();
                        qr.imageFile = QRFile;
                        try
                        {
                            bitmap = new Bitmap(localFileName);
                            decodeQR.getIntentOfQrCode(bitmap);
                            URL = decodeQR.Urlvalue;
                            websiteLink = decodeQR.websiteLink;
                            isPaymentLink = decodeQR.isPaymentLink;
                            merchantName = decodeQR.merchantName;
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }


                    //GetQRCodeDetails(QRFile);

                    //gettext(localFileName);
                    var a = result;

                    if (IsMalicious)
                    {
                        maliciousString = "yes";
                    }
                    else
                    {
                        maliciousString = "no";
                    }
                    string path = localFileName;




                    try
                    {
                        string startupPath = Environment.CurrentDirectory;

                        string path0 = startupPath + "\\QRData.csv";

                        string[] lines = File.ReadAllLines(path0);

                        foreach (string line in lines)
                        {
                            string[] columns = line.Split(',');
                            bool isAnyEmptycolumn = columns.Any(val => string.IsNullOrEmpty(val));

                            if ((columns.Length > 0))
                            {
                                string[] col = columns[0].Split("\\");
                                string name = col[col.Length - 1] + ".png";
                                string url = columns[1];
                                if (name == file.Name)
                                {
                                    maliciousString = columns[2];
                                    //URL = columns[1];
                                    Details = columns[3];
                                    ThreatType = columns[5];
                                    maliciousnesslevel = columns[4];
                                }
                                if (URL.Contains(url))
                                {
                                    maliciousString = columns[2];
                                    //URL = columns[1];
                                    Details = columns[3];
                                    ThreatType = columns[5];
                                    maliciousnesslevel = columns[4];
                                }

                            }
                        }
                    }
                    catch
                    {

                    }

                    replyText += $"URL: {URL}\r\n";
                    replyText += $"IsMalicious: {maliciousString}.\r\n";
                    replyText += $"Maliciousness level: {maliciousnesslevel}\r\n";
                    replyText += $"Threat Type: {ThreatType}\r\n";
                    replyText += $"Details: {Details}\r\n";
                    replyText += $"websiteLink: {websiteLink}\r\n";
                    replyText += $"isPaymentLink: {isPaymentLink.ToString()}\r\n";
                    if (isPaymentLink == true)
                    { replyText += $"merchantName: {merchantName}\r\n"; }


                }
            }
            catch
            {
                replyText += $"URL: {URL}\r\n";
                replyText += $"IsMalicious: {maliciousString}.\r\n";
                replyText += $"Maliciousness level: {maliciousnesslevel}\r\n";
                replyText += $"Threat Type: {ThreatType}\r\n";
                replyText += $"Details: {Details}\r\n";
                replyText += $"websiteLink: {websiteLink}\r\n";
                replyText += $"isPaymentLink: {isPaymentLink.ToString()}\r\n";
                if (isPaymentLink == true)
                { replyText += $"merchantName: {merchantName}\r\n"; }
            }

            return MessageFactory.Text(replyText);
        }
    }
}
