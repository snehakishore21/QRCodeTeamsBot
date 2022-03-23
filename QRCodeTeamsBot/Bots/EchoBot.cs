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

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = await ProcessInput(turnContext, cancellationToken);
            await turnContext.SendActivityAsync("HI");
            // Respond to the user.
            await turnContext.SendActivityAsync(reply, cancellationToken);
            var card = new HeroCard
            {
                Text = "Do you want to continue opening this QR Code URL/Action?",
                Buttons = new List<CardAction>
                {
                    // Note that some channels require different values to be used in order to get buttons to display text.
                    // In this code the emulator is accounted for with the 'title' parameter, but in other channels you may
                    // need to provide a value for other parameters like 'text' or 'displayText'.
                    new CardAction(ActionTypes.ImBack, title: "1. Yes", value: "1"),
                    new CardAction(ActionTypes.ImBack, title: "2. No", value: "2"),
                },
            };

            var reply2 = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(reply2, cancellationToken);
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
                    await turnContext.SendActivityAsync(
                        $"Welcome to Malicious QR Code Detector Bot {member.Name}." +
                        $" This bot will inform you to whether your QR Code is malicious or not." +
                        $" Please select an option",
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

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                // We know the user is sending an attachment as there is at least one item
                // in the Attachments list.
                reply = HandleIncomingAttachment(activity);
                var card = new HeroCard
                {
                    Text = "You can upload an QR Code to check whether its malicious or not.",
                };
            }
            else
            {
                // Send at attachment to the user.
                reply = await HandleOutgoingAttachment(turnContext, activity, cancellationToken);
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
                reply.Attachments = new List<Attachment>() { GetInlineAttachment() };
            }
            else if (activity.Text.StartsWith("2"))
            {
                reply = MessageFactory.Text("This is an attachment from a HTTP URL.");
                reply.Attachments = new List<Attachment>() { GetInternetAttachment() };
            }
            else if (activity.Text.StartsWith("3"))
            {
                reply = MessageFactory.Text("This is an uploaded attachment.");

                // Get the uploaded attachment.
                var uploadedAttachment = await GetUploadedAttachmentAsync(turnContext, activity.ServiceUrl, activity.Conversation.Id, cancellationToken);
                reply.Attachments = new List<Attachment>() { uploadedAttachment };
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

        public static async void  gettext(string localFileName)
        #pragma warning restore CA1822 
        // Mark members as static
        {
            var responseText = "";
            try
            {
                string jsonString = localFileName;

                /*client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header */


                /*Console.WriteLine("Response: {0}", jsonString);
                FormFile QRFile;
                using (var stream = System.IO.File.OpenRead(jsonString))
                {
                    QRFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
                }
                QRCode qr = new QRCode();
                qr.Id = 1;
                qr.Name = "a";
                qr.imageFile = QRFile;

                HttpClient client = new HttpClient();

                var putUrl = @"https://maliciousqrdetector.azurewebsites.net/QRBitmaps/Search";
                byte[] data;
                using (var br = new BinaryReader(qr.imageFile.OpenReadStream()))
                {
                    data = br.ReadBytes((int)qr.imageFile.OpenReadStream().Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                MultipartFormDataContent multiContent = new MultipartFormDataContent();
                multiContent.Add(bytes, "file", qr.imageFile.FileName);
                multiContent.Add(new StringContent(qr.Id.ToString()), "Id");
                multiContent.Add(new StringContent(qr.Name), "Name");
                var response = await client.PutAsync(putUrl, multiContent);

                var responseText2 = await response.Content.ReadAsStringAsync().ConfigureAwait(true); //right!
                Console.WriteLine("Response: {0}", responseText2); 
                */

                /* var client = new RestSharp.RestClient("https://maliciousqrdetector.azurewebsites.net/QRBitmaps");
                 var request = new RestSharp.RestRequest("AssessQR", RestSharp.Method.Post) { RequestFormat = RestSharp.DataFormat.Json, AlwaysMultipartFormData = true };
                 request.AddParameter("file", localFileName);
                 var uploadDocumentBlobResponse = client.ExecuteAsync(request);*/

                if(!File.Exists(localFileName))
                    throw new FileNotFoundException();

                FormFile QRFile;
                using (var stream = System.IO.File.OpenRead(jsonString))
                {
                    QRFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
                }
                QRCode qr = new QRCode();
                qr.imageFile = QRFile;

                HttpClient client = new HttpClient();

                var postUrl = @"https://maliciousqrdetector.azurewebsites.net/QRBitmaps/QRAssess";
                byte[] data;
                using (var br = new BinaryReader(qr.imageFile.OpenReadStream()))
                {
                    data = br.ReadBytes((int)qr.imageFile.OpenReadStream().Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                MultipartFormDataContent multiContent = new MultipartFormDataContent();
                multiContent.Add(bytes, "ImageFile", QRFile.FileName);
                var response = await client.PostAsync(postUrl, multiContent);

                var responseText2 = await response.Content.ReadAsStringAsync().ConfigureAwait(true); //right!
                Console.WriteLine("Response: {0}", responseText2);

                /*if (!File.Exists(localFileName))
                    throw new FileNotFoundException();
                var data = JsonConvert.SerializeObject(new UploadedFile(localFileName));
                using (var client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    string response = await client.UploadStringTaskAsync(new Uri("https://maliciousqrdetector.azurewebsites.net/QRBitmaps/AssessQR"), "POST", data);
                    result = response;
                }*/


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage responseTask = await client.GetAsync("https://api.publicapis.org/entries");
                responseText = await responseTask.Content.ReadAsStringAsync().ConfigureAwait(true); //right!
                if (responseTask.IsSuccessStatusCode)
                {
                    responseText = await responseTask.Content.ReadAsStringAsync().ConfigureAwait(true); //right                                                                                                      // result = await responseTask.Content.ReadAsAsync<string>();
                }
            }
            catch { }
            try
            {
                /*var service = new SafebrowsingService(new BaseClientService.Initializer
                {
                    ApplicationName = "dotnet-client",
                    ApiKey = "API-KEY"
                });

                var request = service.ThreatMatches.Find(new FindThreatMatchesRequest()
                {
                    Client = new ClientInfo
                    {
                        ClientId = "Dotnet-client",
                        ClientVersion = "1.5.2"
                    },
                    ThreatInfo = new ThreatInfo()
                    {
                        ThreatTypes = new List<string> { "Malware" },
                        PlatformTypes = new List<string> { "Windows" },
                        ThreatEntryTypes = new List<string> { "URL" },
                        ThreatEntries = new List<ThreatEntry>
                {
                    new ThreatEntry
                    {
                        Url = "google.com"
                    }
                }
                    }
                });

                var response = await request.ExecuteAsync();
                */

                
            }
            catch { }

            result = responseText;
        }


        // Handle attachments uploaded by users. The bot receives an <see cref="Attachment"/> in an <see cref="Activity"/>.
        // The activity has a "IList{T}" of attachments.    
        // Not all channels allow users to upload files. Some channels have restrictions
        // on file type, size, and other attributes. Consult the documentation for the channel for
        // more information. For example Skype's limits are here
        // <see ref="https://support.skype.com/en/faq/FA34644/skype-file-sharing-file-types-size-and-time-limits"/>.
        private static IMessageActivity HandleIncomingAttachment(IMessageActivity activity)
        {
            var replyText = string.Empty;
            foreach (var file in activity.Attachments)
            {
                // Determine where the file is hosted.
                var remoteFileUrl = file.ContentUrl;

                // Save the attachment to the system temp directory.
                var localFileName = Path.Combine(Path.GetTempPath(), file.Name);

                // Download the actual attachment
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteFileUrl, localFileName);
                }
                var IsMalicious = true;
                var maliciousString = "";
                gettext(localFileName);
                var a = result;

                if (IsMalicious)
                    {
                    maliciousString = "malicious";
                }
                else
                {
                    maliciousString = " not malicious";
                }

                replyText += $"Attachment \"{file.Name}\"" +
                             $" has been received and saved to \"{localFileName}\"\r\n";
                replyText += $"This QR code is \"{maliciousString}\".\r\n" ;
                replyText += $"Maliciousness level: \"100%\"\r\n";
                replyText += $"Details: \"This QR Code accesses your personal information, location and tries to download malicious files on your local machine.\"\r\n";
                replyText += $"TestDetails: \"{result}\"\r\n";

                //   replyText += $"Details: \"{a}.\"\r\n";
            }

            return MessageFactory.Text(replyText);
        }

        // Creates an inline attachment sent from the bot to the user using a base64 string.
        // Using a base64 string to send an attachment will not work on all channels.
        // Additionally, some channels will only allow certain file types to be sent this way.
        // For example a .png file may work but a .pdf file may not on some channels.
        // Please consult the channel documentation for specifics.
        private static Attachment GetInlineAttachment()
        {
            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png");
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            return new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = $"data:image/png;base64,{imageData}",
            };
        }

        // Creates an "Attachment" to be sent from the bot to the user from an uploaded file.
        private static async Task<Attachment> GetUploadedAttachmentAsync(ITurnContext turnContext, string serviceUrl, string conversationId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png");

            var connector = turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;
            var attachments = new Attachments(connector);
            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                conversationId,
                new AttachmentData
                {
                    Name = @"Resources\architecture-resize.png",
                    OriginalBase64 = File.ReadAllBytes(imagePath),
                    Type = "image/png",
                },
                cancellationToken);

            var attachmentUri = attachments.GetAttachmentUri(response.Id);

            return new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = attachmentUri,
            };
        }

        // Creates an <see cref="Attachment"/> to be sent from the bot to the user from a HTTP URL.
        private static Attachment GetInternetAttachment()
        {
            // ContentUrl must be HTTPS.
            return new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
            };
        }
    }
}
