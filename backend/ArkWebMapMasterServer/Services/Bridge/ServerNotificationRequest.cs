using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Requests;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class ServerNotificationRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Decode the body
            UserNotificationRequest request = Program.DecodePostBody<UserNotificationRequest>(e);

            //Convert the requested notification into a notification object, based on their type.
            FirebaseNotification notification;
            switch (request.notification.type)
            {
                case TribeNotificationType.BabyDinoFoodLow:
                    notification = ConvertDinoFoodLow(request.notification, s);
                    break;
                default:
                    throw new StandardError("Unknown notification type.", StandardErrorCode.InvalidInput);
            }

            //Send this to all tribe members.
            try
            {
                MobileDeviceNotifications.SendNotificationToTribe(s, request.tribeId, notification);
            } catch (Exception ex)
            {
                
            }

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }

        static FirebaseNotification ConvertDinoFoodLow(TribeNotification n, ArkServer s)
        {
            //Unpack
            double foodItemsRemaining = double.Parse(n.data["foodItemsRemaining"]);
            TimeSpan foodTimeRemaining = TimeSpan.FromMilliseconds(double.Parse(n.data["foodTimeRemaining"]));
            string dinoName = n.data["dinoName"];
            string dinoClassname = n.data["dinoClassname"];
            string dinoPronoun = n.data["dinoGender"];

            //Determine DinoHungerSeverity
            DinoHungerSeverity severity;
            if (foodTimeRemaining.TotalMinutes < 1)
                severity = DinoHungerSeverity.Starving;
            else if (foodTimeRemaining.TotalMinutes < 8)
                severity = DinoHungerSeverity.Critical;
            else
                severity = DinoHungerSeverity.Low;

            //Create texts based on severity.
            DinoHungerSeverityStatus status = statuses[severity];
            string body = status.bottom_text.Replace("{dino}", dinoName).Replace("{gender}", dinoPronoun.ToLower()).Replace("{Gender}", dinoPronoun).Replace("{time}", Math.Round(foodTimeRemaining.TotalMinutes, 0).ToString());

            return new FirebaseNotification
            {
                title = $"{dinoName} on Ark '{s.display_name}' is {status.title_name}",
                body = body
            };
        }

        enum DinoHungerSeverity
        {
            Starving, //Minutes < 1
            Critical, //Minutes < 8
            Low //Minutes < 30
        }

        class DinoHungerSeverityStatus
        {
            public string title_name;
            public string bottom_text; //Use {dino} in string for dino name. Use {gender} for gender pronoun. Alternatively, you can use {Gender} for a string beginning with a capital letter. Use {time} for time, in minutes, until the dino will need food.
        }

        static readonly Dictionary<DinoHungerSeverity, DinoHungerSeverityStatus> statuses = new Dictionary<DinoHungerSeverity, DinoHungerSeverityStatus> {
            {
                DinoHungerSeverity.Starving,
                new DinoHungerSeverityStatus
                {
                    title_name = "starving!",
                    bottom_text = "{dino} is losing health."
                }
            },
            {
                DinoHungerSeverity.Critical,
                new DinoHungerSeverityStatus
                {
                    title_name = "very low on food",
                    bottom_text = "{dino} will run out of food in {time} minutes."
                }
            },
            {
                DinoHungerSeverity.Low,
                new DinoHungerSeverityStatus
                {
                    title_name = "getting hungry",
                    bottom_text = "Feed {dino} within {time} minutes."
                }
            }
        };
    }
}
