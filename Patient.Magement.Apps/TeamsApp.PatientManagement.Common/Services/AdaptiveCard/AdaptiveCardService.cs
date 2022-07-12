
namespace TeamsApp.PatientManagement.Common.Services.AdaptiveCard
{
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using TeamsApp.PatientManagement.Common.Models.Configuration;
    using TeamsApp.PatientManagement.Common.Resources;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using TeamsApp.PatientManagement.Common.Models.Ether;
    using System.Linq;
    /// <summary>
    /// Class that helps to return welcome card as attachment.
    /// </summary>
    public class AdaptiveCardService : IAdaptiveCardService
    {
       
        /// <summary>
        /// Memory cache instance to store and retrieve adaptive card payload.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Information about the web hosting environment an application is running in.
        /// </summary>
        private readonly IWebHostEnvironment env;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// A set of key/value application configuration properties for Activity settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        //private readonly int TaskModuleHeightMedium = 500;
        //private readonly int TaskModuleWidthMedium = 700;

        //private readonly int TaskModuleWidthLarge = 900;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveCardService"/> class.
        /// </summary>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="env">Information about the web hosting environment an application is running in.</param>
        /// <param name="memoryCache">MemoryCache instance for caching authorization result.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity handler.</param>
        public AdaptiveCardService(IStringLocalizer<Strings> localizer, IMemoryCache memoryCache, IWebHostEnvironment env, IOptions<BotSettings> botOptions)
        {
            this.localizer = localizer;
            this.botOptions = botOptions;
            this.memoryCache = memoryCache;
            this.env = env;
        }

        private List<AdaptiveElement> BuildINewPatientResponseCardBody(EtherPatientResponseModel patientDetails, string channelId, string teamId, string departmentId, EtherPatientAdmissionDetailsResponseModel admissionDetails)
        {
            var patientDetailsItem = patientDetails.body.FirstOrDefault();
            var admissionNo = "";
            DateTime admissionDate;
            var admissionDateStr = string.Empty;
            if (admissionDetails.body.Any())
            {
                admissionNo = admissionDetails.body[0].Admnumber;
                admissionDate = admissionDetails.body[0].AdmissionDate;
                admissionDateStr = admissionDate.ToString("ddd, dd MMMM yyyy hh:mm tt");
            }
            var cardBodyToConstruct = new List<AdaptiveElement>();
            // Container 0
            var container0 = new AdaptiveContainer()
            {
                Items = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text =Strings.NewPatientCardTopMessage,
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Default
                                    }
                                }
                            }

                        }

                    }

                }
            };
            cardBodyToConstruct.Add(container0);

            // Container 1
            var container1 = new AdaptiveContainer()
            {
                Items = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text =$"New Patient {patientDetailsItem.Salutation} {patientDetailsItem.PatientName}, {admissionNo} Admitted.",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Large,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Separator=true

                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text =Strings.NewPatientCardSubHeading,
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small
                                    }
                                }
                            }
                            
                        }

                    }

                }
            };
            cardBodyToConstruct.Add(container1);

            // Container 2
            List<AdaptiveElement> adaptiveElementList2 = new List<AdaptiveElement>();
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**Patient Name** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {patientDetailsItem.Salutation} {patientDetailsItem.PatientName}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**UHID** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {patientDetailsItem.UHID}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**Admission No.** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {admissionNo}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**DOA** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {admissionDateStr}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });

           
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**Gender** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {patientDetailsItem.Sex}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            
            var container2 = new AdaptiveContainer()
            {
                Separator = true,
                Items = adaptiveElementList2
            };
            cardBodyToConstruct.Add(container2);

            AdaptiveActionSet actionSet1 = new AdaptiveActionSet();
            List<string> buttonSet1 = new List<string>();
            buttonSet1.Add(BotCommandConstants.ViewPatientDetails);
            buttonSet1.Add(BotCommandConstants.ViewVitalsDetails);
            buttonSet1.Add(BotCommandConstants.ViewIntakeOutputDetails);
            actionSet1.Actions = BuildNewPatientResponseCardAction(patientDetails, channelId, teamId, departmentId, buttonSet1);
            var actionSet1Container = new AdaptiveContainer()
            {
                Items = { actionSet1 }
            };
            cardBodyToConstruct.Add(actionSet1Container);

            AdaptiveActionSet actionSet2 = new AdaptiveActionSet();
            List<string> buttonSet2 = new List<string>();
            buttonSet2.Add(BotCommandConstants.ViewPrescriptionDetails);
            buttonSet2.Add(BotCommandConstants.ViewDoctorNotesDetails);
            buttonSet2.Add(BotCommandConstants.ViewLabReportDetails);
            buttonSet2.Add(BotCommandConstants.ViewMedicalHistoryDetails);
            actionSet2.Actions = BuildNewPatientResponseCardAction(patientDetails, channelId, teamId, departmentId, buttonSet2);
            var actionSet2Container = new AdaptiveContainer()
            {
                Items = { actionSet2 }
            };
            cardBodyToConstruct.Add(actionSet2Container);


            return cardBodyToConstruct;
        }

        private List<AdaptiveAction> BuildNewPatientResponseCardAction(EtherPatientResponseModel patientDetails, string channelId,string teamId, string departmentId)
        {
            var patientDetailsItem = patientDetails.body.FirstOrDefault();
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();
           
            //Patient Details Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.PatientDetailsViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewPatientDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId= channelId,
                    TeamId = teamId,
                    AdmissionId= patientDetailsItem.AdmissionId,
                    DepartmentId= departmentId
                }
            });

            //Add Vitals Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.VitalDetailsViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewVitalsDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId = channelId,
                    TeamId = teamId,
                    AdmissionId = patientDetailsItem.AdmissionId,
                    DepartmentId = departmentId
                }
            });
            //Add Intake Output Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.IntakeOutputViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewIntakeOutputDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId = channelId,
                    TeamId = teamId,
                    AdmissionId = patientDetailsItem.AdmissionId,
                    DepartmentId = departmentId
                }
            });
            //Add Doctor notes Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.DoctorNotesViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewDoctorNotesDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId = channelId,
                    TeamId = teamId,
                    AdmissionId = patientDetailsItem.AdmissionId,
                    DepartmentId = departmentId
                }
            });
            //Add Prescription Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.PrescriptionDetailsViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewPrescriptionDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId = channelId,
                    TeamId = teamId,
                    AdmissionId = patientDetailsItem.AdmissionId,
                    DepartmentId = departmentId
                }
            });
            //Add Lab Reports Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.LapReportsViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewLabReportDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId = channelId,
                    TeamId = teamId,
                    AdmissionId = patientDetailsItem.AdmissionId,
                    DepartmentId = departmentId
                }
            });

            //Add Medical History Button
            actionsList.Add(new AdaptiveSubmitAction
            {
                Title = Strings.MedicalHistoryViewButtonText,
                Data = new AdaptiveCardActionModel
                {
                    MsteamsCardAction = new CardAction
                    {
                        Type = BotCommandConstants.FetchActionType,
                    },
                    Command = BotCommandConstants.ViewMedicalHistoryDetails,
                    PatientId = patientDetailsItem.PatientId,
                    UHID = patientDetailsItem.UHID,
                    ChannelId = channelId,
                    TeamId = teamId,
                    AdmissionId = patientDetailsItem.AdmissionId,
                    DepartmentId = departmentId
                }
            });

            return actionsList;


        }

        private List<AdaptiveAction> BuildNewPatientResponseCardAction(EtherPatientResponseModel patientDetails, string channelId, string teamId, string departmentId, List<string> buttons)
        {
            var patientDetailsItem = patientDetails.body.FirstOrDefault();
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();
            //Patient Details Button
            if (buttons.Contains(BotCommandConstants.ViewPatientDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.PatientDetailsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewPatientDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }
            //Add Vitals Button
            if (buttons.Contains(BotCommandConstants.ViewVitalsDetails))
            {
                
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.VitalDetailsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewVitalsDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            //Add Intake Output Button
            if (buttons.Contains(BotCommandConstants.ViewIntakeOutputDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.IntakeOutputViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewIntakeOutputDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            //Add Doctor notes Button
            if (buttons.Contains(BotCommandConstants.ViewDoctorNotesDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.DoctorNotesViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewDoctorNotesDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }
            //Add Prescription Button
            if (buttons.Contains(BotCommandConstants.ViewPrescriptionDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.PrescriptionDetailsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewPrescriptionDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }
            //Add Lab Reports Button
            if (buttons.Contains(BotCommandConstants.ViewLabReportDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.LapReportsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewLabReportDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            //Add Medical History Button
            if (buttons.Contains(BotCommandConstants.ViewMedicalHistoryDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.MedicalHistoryViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewMedicalHistoryDetails,
                        PatientId = patientDetailsItem.PatientId,
                        UHID = patientDetailsItem.UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = patientDetailsItem.AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            return actionsList;


        }

        private AdaptiveAction CreateChatWithUserAction(string messageToSend, string ChatTextButton, string userEmail)
        {
            var encodedMessage = Uri.EscapeDataString(messageToSend);

            return new AdaptiveOpenUrlAction
            {
                Title = ChatTextButton,
                Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userEmail)}&message={encodedMessage}"),
            };
        }

        public Attachment GetNewPatientDetailCard(EtherPatientResponseModel patientDetails,string channelId,string teamId,string departmentId, EtherPatientAdmissionDetailsResponseModel admissionDetails)
        {           
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
            {
                Body = BuildINewPatientResponseCardBody(patientDetails, channelId, teamId, departmentId, admissionDetails)
                //Actions = BuildNewPatientResponseCardAction(patientDetails, channelId,teamId, departmentId),

            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,

            };

        }
        /// <summary>
        /// Get card payload from memory.
        /// </summary>
        /// <param name="cardCacheKey">Card cache key.</param>
        /// <param name="jsonTemplateFileName">File name for JSON adaptive card template.</param>
        /// <returns>Returns adaptive card payload in JSON format.</returns>
        private string GetCardPayload(string cardCacheKey, string jsonTemplateFileName)
        {
            bool isCacheEntryExists = this.memoryCache.TryGetValue(cardCacheKey, out string cardPayload);
            if (!isCacheEntryExists)
            {
                // If cache duration is not specified then by default cache for 12 hours.
                var cacheDurationInHour = TimeSpan.FromHours(this.botOptions.Value.CardCacheDurationInHour);
                cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(12) : cacheDurationInHour;

                var cardJsonFilePath = Path.Combine(this.env.ContentRootPath, $".\\Cards\\{jsonTemplateFileName}");


                cardPayload = File.ReadAllText(cardJsonFilePath);
                this.memoryCache.Set(cardCacheKey, cardPayload, cacheDurationInHour);
            }

            return cardPayload;
        }


        #region CardUpdate 
        public Attachment GetUpdatePatientDetailCard_Update(string PatientId, string UHID, string AdmissionId, string PatientName, string Gender, string AdmissionNo, string DOA, string channelId, string teamId, string departmentId)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
            {
                Body = BuildUpdatePatientResponseCardBody_Update(PatientId,UHID,AdmissionId,PatientName,Gender,AdmissionNo,DOA, channelId, teamId, departmentId)

            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,

            };

        }
        private List<AdaptiveElement> BuildUpdatePatientResponseCardBody_Update(string PatientId,string UHID,string AdmissionId,string PatientName,string Gender,string AdmissionNo,string DOA,string channelId, string teamId, string departmentId)
        {
            var admissionNo = AdmissionNo;
            var admissionDateStr = DOA;
            var cardBodyToConstruct = new List<AdaptiveElement>();
            // Container 0
            var container0 = new AdaptiveContainer()
            {
                Items = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text =Strings.NewPatientCardTopMessage,
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Default
                                    }
                                }
                            }

                        }

                    }

                }
            };
            cardBodyToConstruct.Add(container0);

            // Container 1
            var container1 = new AdaptiveContainer()
            {
                Items = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text =$"New Patient {PatientName}, {admissionNo} Admitted.",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Large,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Separator=true

                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text =Strings.NewPatientCardSubHeading,
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small
                                    }
                                }
                            }

                        }

                    }

                }
            };
            cardBodyToConstruct.Add(container1);

            // Container 2
            List<AdaptiveElement> adaptiveElementList2 = new List<AdaptiveElement>();
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**Patient Name** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {PatientName}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**UHID** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {UHID}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**Admission No.** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {admissionNo}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });
            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**DOA** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {admissionDateStr}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });


            adaptiveElementList2.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="auto",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text ="**Gender** :",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            },
                             new AdaptiveColumn()
                            {
                                Width="stretch",
                                Spacing=AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock
                                    {
                                        Text =$" {Gender}",
                                        Wrap = true,
                                        Size=AdaptiveTextSize.Small,
                                        Spacing=AdaptiveSpacing.None
                                    }
                                }
                            }
                        }

            });

            var container2 = new AdaptiveContainer()
            {
                Separator = true,
                Items = adaptiveElementList2
            };
            cardBodyToConstruct.Add(container2);

            AdaptiveActionSet actionSet1 = new AdaptiveActionSet();
            List<string> buttonSet1 = new List<string>();
            buttonSet1.Add(BotCommandConstants.ViewPatientDetails);
            buttonSet1.Add(BotCommandConstants.ViewVitalsDetails);
            buttonSet1.Add(BotCommandConstants.ViewIntakeOutputDetails);
            actionSet1.Actions = BuildUpdatePatientResponseCardAction_Update(PatientId,UHID,AdmissionId, channelId, teamId, departmentId, buttonSet1);
            var actionSet1Container = new AdaptiveContainer()
            {
                Items = { actionSet1 }
            };
            cardBodyToConstruct.Add(actionSet1Container);

            AdaptiveActionSet actionSet2 = new AdaptiveActionSet();
            List<string> buttonSet2 = new List<string>();
            buttonSet2.Add(BotCommandConstants.ViewPrescriptionDetails);
            buttonSet2.Add(BotCommandConstants.ViewDoctorNotesDetails);
            buttonSet2.Add(BotCommandConstants.ViewLabReportDetails);
            buttonSet2.Add(BotCommandConstants.ViewMedicalHistoryDetails);
            actionSet2.Actions = BuildUpdatePatientResponseCardAction_Update(PatientId, UHID, AdmissionId, channelId, teamId, departmentId, buttonSet2);
            var actionSet2Container = new AdaptiveContainer()
            {
                Items = { actionSet2 }
            };
            cardBodyToConstruct.Add(actionSet2Container);


            return cardBodyToConstruct;
        }
        private List<AdaptiveAction> BuildUpdatePatientResponseCardAction_Update(string PatientId, string UHID,string AdmissionId,string channelId, string teamId, string departmentId, List<string> buttons)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();
            //Patient Details Button
            if (buttons.Contains(BotCommandConstants.ViewPatientDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.PatientDetailsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewPatientDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }
            //Add Vitals Button
            if (buttons.Contains(BotCommandConstants.ViewVitalsDetails))
            {

                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.VitalDetailsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewVitalsDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            //Add Intake Output Button
            if (buttons.Contains(BotCommandConstants.ViewIntakeOutputDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.IntakeOutputViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewIntakeOutputDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            //Add Doctor notes Button
            if (buttons.Contains(BotCommandConstants.ViewDoctorNotesDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.DoctorNotesViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewDoctorNotesDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }
            //Add Prescription Button
            if (buttons.Contains(BotCommandConstants.ViewPrescriptionDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.PrescriptionDetailsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewPrescriptionDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }
            //Add Lab Reports Button
            if (buttons.Contains(BotCommandConstants.ViewLabReportDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.LapReportsViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewLabReportDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            //Add Medical History Button
            if (buttons.Contains(BotCommandConstants.ViewMedicalHistoryDetails))
            {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = Strings.MedicalHistoryViewButtonText,
                    Data = new AdaptiveCardActionModel
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = BotCommandConstants.FetchActionType,
                        },
                        Command = BotCommandConstants.ViewMedicalHistoryDetails,
                        PatientId = PatientId,
                        UHID = UHID,
                        ChannelId = channelId,
                        TeamId = teamId,
                        AdmissionId = AdmissionId,
                        DepartmentId = departmentId
                    }
                });
            }

            return actionsList;


        }

        #endregion


    }
}