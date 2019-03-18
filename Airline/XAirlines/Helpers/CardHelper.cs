﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AdaptiveCards;
using Airlines.XAirlines.Models;
using Microsoft.Bot.Connector;
using Bogus;
using Newtonsoft.Json;
using Airlines.XAirlines.Common;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using v = Airlines.XAirlines.Helpers;

namespace Airlines.XAirlines.Helpers
{
    public class CardHelper
    {
        public static async Task<Attachment> GetWeeklyRosterCard()
        {
            Common crewdata = new Common();
            Crew crew = crewdata.ReadJson();
            DateTime today = DateTime.Today;
            DateTime weekafter = today.AddDays(6);
            var weekplan = crew.plan.Where(c => c.date >= today && c.date <= weekafter);
            var listCard = new ListCard();
            listCard.content = new Content();
            listCard.content.title = "Here is your next week's roster";
            var list = new List<Item>();
            foreach (var i in weekplan)
            {
                var item = new Item();
                item.id = i.flightDetails.flightStartDate;
                item.type = "resultItem";
                item.icon = ApplicationSettings.BaseUrl + "/Resources/Flight.png";
                item.title = i.flightDetails.flightStartDate + "-" + i.flightDetails.flightEndDate;
                item.subtitle = i.flightDetails.sourceCode + "-" + i.flightDetails.destinationCode;
                item.tap = new Tap()
                {
                    type = ActionTypes.MessageBack,
                    title = "Id",
                    value = JsonConvert.SerializeObject(new AirlineActionDetails()
                    { Id = item.id, ActionType = Constants.ShowDetailedRoster })
                };
                list.Add(item);


            }

            listCard.content.items = list.ToArray();

            Attachment attachment = new Attachment();
            attachment.ContentType = listCard.contentType;
            attachment.Content = listCard.content;
            return attachment;

        }
        public static async Task<Attachment> GetMonthlyRosterCard()
        {


            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {

                Body = new List<AdaptiveElement>()
                     {

                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text="please access the portal tab to view the monthly roster",
                                Wrap=true
                            }
                        }
                    }
                },
            };
            Card.Actions.Add(new AdaptiveOpenUrlAction()
            {
                Title="View Crew Portal",
                Url = new System.Uri(Constants.PortalTabDeeplink)
            });

           

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };


        }
        public static async Task<Attachment> GetWelcomeScreen(string userName)
        {


            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {

                Body = new List<AdaptiveElement>()
                     {

                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveTextBlock()
                            {
                                Text=$"Hey {userName}! Here is what I can do for you",
                                Size=AdaptiveTextSize.Large
                            },
                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                   
                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveTextBlock(){Text="View Weekly roster",Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium, Spacing=AdaptiveSpacing.None, HorizontalAlignment=AdaptiveHorizontalAlignment.Center}
                                         },
                                           SelectAction = new AdaptiveSubmitAction()
                                         {
                                            Data=new ActionDetails(){ActionType=Constants.NextWeekRoster}
                                         }
                                    }
                                }
                            },
                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                    

                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveTextBlock(){Text="View Monthly Roster",Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None }
                                         },
                                           SelectAction = new AdaptiveSubmitAction()
                                         {
                                              Data=new ActionDetails(){ActionType=Constants.NextMonthRoster}
                                         }
                                    }
                                }
                            },
                        }
                    }
                }

            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };


        }
        public static async Task<Attachment> GetDetailedRoster(Activity activity)
        {
            var details = JsonConvert.DeserializeObject<AirlineActionDetails>(activity.Value.ToString());
            Common crewdata = new Common();
            Crew crew = crewdata.ReadJson();
            var datePlan = crew.plan.Where(c => c.flightDetails.flightStartDate == details.Id).FirstOrDefault();
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {

                Body = new List<AdaptiveElement>()
                     {
                    new AdaptiveColumnSet()
                    {
                        Spacing=AdaptiveSpacing.Small,
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Left,
                                        Spacing=AdaptiveSpacing.Small,
                                        Separator=true,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Color=AdaptiveTextColor.Attention,
                                        Text="E0370",
                                        MaxLines=1
                                    }
                                },

                            },
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,

                                        Separator=true,

                                        Text="Updated 2 days ago",

                                    }
                                },

                            },
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Separator=true,
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                               Items=new List<AdaptiveElement>()
                               {
                                   new AdaptiveContainer()
                                   {
                                       Items=new List<AdaptiveElement>()
                                       {
                                            new AdaptiveTextBlock()
                                            {
                                               Size=AdaptiveTextSize.Medium,
                                                Weight=AdaptiveTextWeight.Bolder,
                                               Text=datePlan.flightDetails.flightStartDate
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                               Size=AdaptiveTextSize.Small,
                                               Weight=AdaptiveTextWeight.Bolder,
                                               Text=datePlan.flightDetails.flightDepartueTime
                                            },
                                       }
                                   }


                               }
                            },
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                          new AdaptiveTextBlock()
                                            {
                                               Size=AdaptiveTextSize.Medium,
                                               HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                                Weight=AdaptiveTextWeight.Bolder,
                                               Text=datePlan.flightDetails.flightEndDate
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                                HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                               Size=AdaptiveTextSize.Small,
                                               Weight=AdaptiveTextWeight.Bolder,
                                               Text=datePlan.flightDetails.flightArrivalTime
                                            },
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                      Separator=true,
                      Columns=new List<AdaptiveColumn>()
                      {
                          new AdaptiveColumn()
                          {
                              Items=new List<AdaptiveElement>()
                              {
                                 new AdaptiveTextBlock()
                                 {
                                     HorizontalAlignment=AdaptiveHorizontalAlignment.Center,
                                     Size=AdaptiveTextSize.Medium,
                                     Weight=AdaptiveTextWeight.Bolder,
                                     Text=datePlan.flightDetails.travelDuraion
                                 }
                              }
                          }
                      }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns=new List<AdaptiveColumn>()
                        
                        {
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveContainer()
                                    {
                                        Items=new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Size=AdaptiveTextSize.Small,
                                                Weight=AdaptiveTextWeight.Lighter,
                                                Text=datePlan.flightDetails.source//Need to change
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                                Size=AdaptiveTextSize.ExtraLarge,
                                                Color=AdaptiveTextColor.Accent,
                                                Text=datePlan.flightDetails.sourceCode
                                            }
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Size=AdaptiveTextSize.Small,
                                        Weight=AdaptiveTextWeight.Lighter,
                                        Text=datePlan.flightDetails.destination//Need to change
                                    
                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Size=AdaptiveTextSize.ExtraLarge,
                                       Color=AdaptiveTextColor.Accent,
                                        Text=datePlan.flightDetails.destinationCode

                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Size=AdaptiveTextSize.Medium,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Color=AdaptiveTextColor.Accent,
                                        Text=datePlan.flightDetails.layOVer

                                    },
                                }
                            }
                        }
                        
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text="E-Gate Open"
                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        Text="Block hrs"
                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        Text="Away from base"
                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        Text="AC type"
                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        Text="Tail No"
                                    },
                                },
                                
                            },
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Text=datePlan.flightDetails.gateOpensAt
                                    
                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Text="12h 36 m"//Hard coded

                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Text="05:00,23 Sep"//Hard coded

                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Text=datePlan.flightDetails.acType

                                    },
                                    new AdaptiveTextBlock()
                                    {
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Right,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Text=datePlan.flightDetails.tailNo
                                    },
                                }
                            }
                        },
                        
                    }


                },
                Actions=new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction()
                    {
                        Title="Weather Report",
                        Data=new WeatherActionDetails(){City=datePlan.flightDetails.destination,ActionType=Constants.WeatherCard}
                    },
                    new AdaptiveSubmitAction()
                    {
                        Title="Currency Details",
                        Data=new ActionDetails(){ActionType=Constants.NextMonthRoster}
                    }
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };
        }
        //public static async Task<Attachment> GetDetailedRoster()
        //{
        //    // Parse the JSON 
        //    AdaptiveCardParseResult result = AdaptiveCard.FromJson(GetAdaptiveCardJson());

        //    return new Attachment()
        //    {
        //        ContentType = AdaptiveCard.ContentType,
        //        Content = result.Card

        //    };
        //}
        public static async Task<Attachment> GetUpdateScreen()
        {

            DateTime dateTime;
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {

                Body = new List<AdaptiveElement>()
                {

                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveTextBlock()
                                    {
                                      Size=AdaptiveTextSize.Medium,
                                      Weight=AdaptiveTextWeight.Bolder,
                                      Text="Your monthly roster"
                                    },
                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                    
                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             
                                             new AdaptiveTextBlock(){Text="New roster for the month of "+DateTime.Now.ToString("MMMM")+" has been released.Please acknowledge to view your new roster",Wrap=true}
                                         },
                                          
                                    },
                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveImage(){Url=new Uri(ApplicationSettings.BaseUrl + "/Resources/clipboard.PNG"),Size=AdaptiveImageSize.Auto}
                                         }
                                    },
                                }
                            }
                            
                        }
                    }
                }

            };
            Card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "View Crew Portal",
                Data=new ActionDetails() { ActionType=Constants.NextWeekRoster}
            });
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };


        }
        public static async Task<Attachment> GetWeatherCard(WeatherInfo weather)
        {

            DateTime dateTime;
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {

                Body = new List<AdaptiveElement>()
                {

                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveTextBlock()
                                    {
                                      Size=AdaptiveTextSize.Medium,
                                      Weight=AdaptiveTextWeight.Bolder,
                                      Text="Your monthly roster"
                                    },
                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {

                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {

                                             new AdaptiveTextBlock(){Text="New roster for the month of "+DateTime.Now.ToString("MMMM")+" has been released.Please acknowledge to view your new roster",Wrap=true}
                                         },

                                    },
                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveImage(){Url=new Uri(ApplicationSettings.BaseUrl + "/Resources/clipboard.PNG"),Size=AdaptiveImageSize.Auto}
                                         }
                                    },
                                }
                            }

                        }
                    }
                }

            };
            Card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "View Crew Portal",
                Data = new ActionDetails() { ActionType = Constants.NextWeekRoster }
            });
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };


        }
        public static async Task<Attachment> GetCurrencyCard()
        {

            DateTime dateTime;
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {

                Body = new List<AdaptiveElement>()
                {

                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveTextBlock()
                                    {
                                      Size=AdaptiveTextSize.Medium,
                                      Weight=AdaptiveTextWeight.Bolder,
                                      Text="Your monthly roster"
                                    },
                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {

                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {

                                             new AdaptiveTextBlock(){Text="New roster for the month of "+DateTime.Now.ToString("MMMM")+" has been released.Please acknowledge to view your new roster",Wrap=true}
                                         },

                                    },
                                    new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveImage(){Url=new Uri(ApplicationSettings.BaseUrl + "/Resources/clipboard.PNG"),Size=AdaptiveImageSize.Auto}
                                         }
                                    },
                                }
                            }

                        }
                    }
                }

            };
            Card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "View Crew Portal",
                Data = new ActionDetails() { ActionType = Constants.NextWeekRoster }
            });
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };


        }
        public static String GetAdaptiveCardJson()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/Cards/AdaptiveCard.json");
            return File.ReadAllText(path);
        }
    }
}