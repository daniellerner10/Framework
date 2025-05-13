using System.Text;
using Keeper.Framework.Extensions.Collections;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Keeper.Framework.Serialization.Xml.Tests
{
    public class XmlSerializerExtensionsShould
    {
        [Fact]
        public void SerializeContractCorrectly()
        {
            var req = new GetVisitChangesV2Request
            {
                Authentication = new()
                {
                    AppKey = "key",
                    AppName = "name",
                    AppSecret = "secret"
                },
                ModifiedAfter = DateTime.Parse("2020-01-01")
            };

            var xml = req.SerializeToXml(indent: true);

            Assert.Equal(
              """
              <GetVisitChangesV2Request xmlns="https://www.hhaexchange.com/apis/hhaws.integration">
                <Authentication>
                  <AppName>name</AppName>
                  <AppSecret>secret</AppSecret>
                  <AppKey>key</AppKey>
                </Authentication>
                <ModifiedAfter>2020-01-01T00:00:00</ModifiedAfter>
              </GetVisitChangesV2Request>
              """,
              xml
            );
        }

        [Fact]
        public void SerializeContractAsSoapCorrectly()
        {
            var req = new GetVisitChangesV2Request
            {
                Authentication = new()
                {
                    AppKey = "key",
                    AppName = "name",
                    AppSecret = "secret"
                },
                ModifiedAfter = DateTime.Parse("2020-01-01")
            };

            var xml = req.SerializeSoapRequest();

            Assert.Equal(
              """
              <?xml version="1.0" encoding="utf-8"?>
              <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                <soap:Body>
                  <GetVisitChangesV2Request xmlns="https://www.hhaexchange.com/apis/hhaws.integration">
                    <Authentication>
                      <AppName>name</AppName>
                      <AppSecret>secret</AppSecret>
                      <AppKey>key</AppKey>
                    </Authentication>
                    <ModifiedAfter>2020-01-01T00:00:00</ModifiedAfter>
                  </GetVisitChangesV2Request>
                </soap:Body>
              </soap:Envelope>
              """,
              xml
            );
        }

        [Fact]
        public void DeserializeXmlStreamCorrectly()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Xml));

            var list = stream
              .DeserializeFromXml<GetVisitChangesDetails>("GetVisitChangesV2Info")
              .ToList();

            Assert.Collection(list,
              x =>
              {
                  Assert.Equal(1001, x.VisitID);
                  Assert.Equal(3001, x.Patient.ID);
              },
              x =>
              {
                  Assert.Equal(1002, x.VisitID);
                  Assert.Equal(3002, x.Patient.ID);
              }
            );
        }

        [Fact]
        public void DeserializeSoapXmlStreamCorrectly()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SoapXml));

            var list = stream
              .DeserializeFromXml<GetVisitChangesDetails>("GetVisitChangesV2Info")
              .ToList();

            Assert.Collection(list,
              x =>
              {
                  Assert.Equal(1001, x.VisitID);
                  Assert.Equal(3001, x.Patient.ID);
              },
              x =>
              {
                  Assert.Equal(1002, x.VisitID);
                  Assert.Equal(3002, x.Patient.ID);
              }
            );
        }

        [Fact]
        public async Task DeserializeSoapXmlStreamAsyncCorrectly()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SoapXml));

            var list = await stream
              .DeserializeFromXmlAsync<GetVisitChangesDetails>("GetVisitChangesV2Info")
              .ToListAsync();

            Assert.Collection(list,
              x =>
              {
                  Assert.Equal(1001, x.VisitID);
                  Assert.Equal(3001, x.Patient.ID);
              },
              x =>
              {
                  Assert.Equal(1002, x.VisitID);
                  Assert.Equal(3002, x.Patient.ID);
              }
            );
        }

        [Fact]
        public async Task DeserializeXmlStreamAsyncCorrectly()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Xml));

            var list = await stream
              .DeserializeFromXmlAsync<GetVisitChangesDetails>("GetVisitChangesV2Info")
              .ToListAsync();

            Assert.Collection(list,
              x =>
              {
                  Assert.Equal(1001, x.VisitID);
                  Assert.Equal(3001, x.Patient.ID);
              },
              x =>
              {
                  Assert.Equal(1002, x.VisitID);
                  Assert.Equal(3002, x.Patient.ID);
              }
            );
        }

        private const string Xml = """
          <GetVisitChangesV2Response
              xmlns="https://www.hhaexchange.com/apis/hhaws.integration" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
              <GetVisitChangesV2Result>
                  <GetVisitChangesV2Info>
                      <VisitID>1001</VisitID>
                      <LastModifiedDate>2025-02-28T14:30:00Z</LastModifiedDate>
                      <OfficeID>501</OfficeID>
                      <VisitDate>2025-02-28</VisitDate>
                      <Patient>
                          <ID>3001</ID>
                          <AdmissionNumber>A12345</AdmissionNumber>
                          <FirstName>John</FirstName>
                          <LastName>Doe</LastName>
                      </Patient>
                      <Caregiver>
                          <ID>2001</ID>
                          <FirstName>Jane</FirstName>
                          <LastName>Smith</LastName>
                          <CaregiverCode>C123</CaregiverCode>
                          <TimeAndAttendancePIN>5678</TimeAndAttendancePIN>
                          <PayCode xsi:nil="true" />
                      </Caregiver>
                      <ScheduleStartTime>08:00</ScheduleStartTime>
                      <ScheduleEndTime>12:00</ScheduleEndTime>
                      <VisitStartTime>08:10</VisitStartTime>
                      <VisitEndTime>12:05</VisitEndTime>
                      <EVVStartTime>08:12</EVVStartTime>
                      <EVVEndTime>12:03</EVVEndTime>
                      <IsMissedVisit>No</IsMissedVisit>
                      <TTOT>
                          <Hours>4</Hours>
                          <Minutes>0</Minutes>
                      </TTOT>
                      <Verification>
                          <VerifiedBy>Supervisor</VerifiedBy>
                          <Notes>Visit completed successfully</Notes>
                          <VerifiedDate>2025-02-28T15:00:00Z</VerifiedDate>
                          <VerifiedTime>15:00</VerifiedTime>
                          <SupervisorName>Emily Johnson</SupervisorName>
                      </Verification>
                      <Timesheet>
                          <Required>Yes</Required>
                          <Approved>Yes</Approved>
                      </Timesheet>
                      <TaskPerformed>
                          <POCTaskCode>101</POCTaskCode>
                          <AdditionalValue>1.5</AdditionalValue>
                          <Status>Completed</Status>
                          <Category>Personal Care</Category>
                          <Duty>Bathing Assistance</Duty>
                          <Minutes>30</Minutes>
                      </TaskPerformed>
                      <VisitType>Routine</VisitType>
                      <ClockInEvvType>Mobile</ClockInEvvType>
                      <ClockOutEvvType>Mobile</ClockOutEvvType>
                      <ScheduleDuration>
                          <ScheduleDurationHours>4</ScheduleDurationHours>
                          <ScheduleDurationMinutes>0</ScheduleDurationMinutes>
                      </ScheduleDuration>
                      <BilledAmount>150.00</BilledAmount>
                      <BudgetNumber>5001</BudgetNumber>
                      <ActualHours>3.92</ActualHours>
                      <ActualHoursRounded>4</ActualHoursRounded>
                      <PayHours>3.92</PayHours>
                      <PayHoursUnrounded>3.92</PayHoursUnrounded>
                      <AdjustedHours>4</AdjustedHours>
                      <TimeZone>EST</TimeZone>
                  </GetVisitChangesV2Info>
                  <GetVisitChangesV2Info>
                      <VisitID>1002</VisitID>
                      <LastModifiedDate>2025-02-28T16:45:00Z</LastModifiedDate>
                      <OfficeID>502</OfficeID>
                      <VisitDate>2025-02-28</VisitDate>
                      <Patient>
                          <ID>3002</ID>
                          <AdmissionNumber>A67890</AdmissionNumber>
                          <FirstName>Alice</FirstName>
                          <LastName>Johnson</LastName>
                      </Patient>
                      <Caregiver>
                          <ID>2002</ID>
                          <FirstName>Michael</FirstName>
                          <LastName>Brown</LastName>
                          <CaregiverCode>C456</CaregiverCode>
                          <TimeAndAttendancePIN>9876</TimeAndAttendancePIN>
                          <PayCode xsi:nil="true" />
                      </Caregiver>
                      <ScheduleStartTime>14:00</ScheduleStartTime>
                      <ScheduleEndTime>18:00</ScheduleEndTime>
                      <VisitStartTime>14:05</VisitStartTime>
                      <VisitEndTime>18:10</VisitEndTime>
                      <EVVStartTime>14:07</EVVStartTime>
                      <EVVEndTime>18:08</EVVEndTime>
                      <IsMissedVisit>No</IsMissedVisit>
                      <TTOT>
                          <Hours>4</Hours>
                          <Minutes>10</Minutes>
                      </TTOT>
                      <Verification>
                          <VerifiedBy>Supervisor</VerifiedBy>
                          <Notes>Client requested additional assistance</Notes>
                          <VerifiedDate>2025-02-28T19:00:00Z</VerifiedDate>
                          <VerifiedTime>19:00</VerifiedTime>
                          <SupervisorName>Daniel Wilson</SupervisorName>
                      </Verification>
                      <Timesheet>
                          <Required>Yes</Required>
                          <Approved>Yes</Approved>
                      </Timesheet>
                      <TaskPerformed>
                          <POCTaskCode>102</POCTaskCode>
                          <AdditionalValue>2.0</AdditionalValue>
                          <Status>Completed</Status>
                          <Category>Mobility Assistance</Category>
                          <Duty>Wheelchair Support</Duty>
                          <Minutes>45</Minutes>
                      </TaskPerformed>
                      <VisitType>Follow-up</VisitType>
                      <ClockInEvvType>Telephony</ClockInEvvType>
                      <ClockOutEvvType>Telephony</ClockOutEvvType>
                      <ScheduleDuration>
                          <ScheduleDurationHours>4</ScheduleDurationHours>
                          <ScheduleDurationMinutes>0</ScheduleDurationMinutes>
                      </ScheduleDuration>
                      <BilledAmount>160.00</BilledAmount>
                      <BudgetNumber>5002</BudgetNumber>
                      <ActualHours>4.17</ActualHours>
                      <ActualHoursRounded>4</ActualHoursRounded>
                      <PayHours>4.17</PayHours>
                      <PayHoursUnrounded>4.17</PayHoursUnrounded>
                      <AdjustedHours>4</AdjustedHours>
                      <TimeZone>PST</TimeZone>
                  </GetVisitChangesV2Info>
              </GetVisitChangesV2Result>
          </GetVisitChangesV2Response>
          """;

        private const string SoapXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetVisitChangesV2Response xmlns="https://www.hhaexchange.com/apis/hhaws.integration">
                  <GetVisitChangesV2Result>
                    <GetVisitChangesV2Info>
                      <VisitID>1001</VisitID>
                      <LastModifiedDate>2025-02-28T14:30:00Z</LastModifiedDate>
                      <OfficeID>501</OfficeID>
                      <VisitDate>2025-02-28</VisitDate>
                      <Patient>
                        <ID>3001</ID>
                        <AdmissionNumber>A12345</AdmissionNumber>
                        <FirstName>John</FirstName>
                        <LastName>Doe</LastName>
                      </Patient>
                      <Caregiver>
                        <ID>2001</ID>
                        <FirstName>Jane</FirstName>
                        <LastName>Smith</LastName>
                        <CaregiverCode>C123</CaregiverCode>
                        <TimeAndAttendancePIN>5678</TimeAndAttendancePIN>
                        <PayCode xsi:nil="true" />
                      </Caregiver>
                      <ScheduleStartTime>08:00</ScheduleStartTime>
                      <ScheduleEndTime>12:00</ScheduleEndTime>
                      <VisitStartTime>08:10</VisitStartTime>
                      <VisitEndTime>12:05</VisitEndTime>
                      <EVVStartTime>08:12</EVVStartTime>
                      <EVVEndTime>12:03</EVVEndTime>
                      <IsMissedVisit>No</IsMissedVisit>
                      <TTOT>
                        <Hours>4</Hours>
                        <Minutes>0</Minutes>
                      </TTOT>
                      <Verification>
                        <VerifiedBy>Supervisor</VerifiedBy>
                        <Notes>Visit completed successfully</Notes>
                        <VerifiedDate>2025-02-28T15:00:00Z</VerifiedDate>
                        <VerifiedTime>15:00</VerifiedTime>
                        <SupervisorName>Emily Johnson</SupervisorName>
                      </Verification>
                      <Timesheet>
                        <Required>Yes</Required>
                        <Approved>Yes</Approved>
                      </Timesheet>
                      <TaskPerformed>
                        <POCTaskCode>101</POCTaskCode>
                        <AdditionalValue>1.5</AdditionalValue>
                        <Status>Completed</Status>
                        <Category>Personal Care</Category>
                        <Duty>Bathing Assistance</Duty>
                        <Minutes>30</Minutes>
                      </TaskPerformed>
                      <VisitType>Routine</VisitType>
                      <ClockInEvvType>Mobile</ClockInEvvType>
                      <ClockOutEvvType>Mobile</ClockOutEvvType>
                      <ScheduleDuration>
                        <ScheduleDurationHours>4</ScheduleDurationHours>
                        <ScheduleDurationMinutes>0</ScheduleDurationMinutes>
                      </ScheduleDuration>
                      <BilledAmount>150.00</BilledAmount>
                      <BudgetNumber>5001</BudgetNumber>
                      <ActualHours>3.92</ActualHours>
                      <ActualHoursRounded>4</ActualHoursRounded>
                      <PayHours>3.92</PayHours>
                      <PayHoursUnrounded>3.92</PayHoursUnrounded>
                      <AdjustedHours>4</AdjustedHours>
                      <TimeZone>EST</TimeZone>
                    </GetVisitChangesV2Info>
                    <GetVisitChangesV2Info>
                      <VisitID>1002</VisitID>
                      <LastModifiedDate>2025-02-28T16:45:00Z</LastModifiedDate>
                      <OfficeID>502</OfficeID>
                      <VisitDate>2025-02-28</VisitDate>
                      <Patient>
                        <ID>3002</ID>
                        <AdmissionNumber>A67890</AdmissionNumber>
                        <FirstName>Alice</FirstName>
                        <LastName>Johnson</LastName>
                      </Patient>
                      <Caregiver>
                        <ID>2002</ID>
                        <FirstName>Michael</FirstName>
                        <LastName>Brown</LastName>
                        <CaregiverCode>C456</CaregiverCode>
                        <TimeAndAttendancePIN>9876</TimeAndAttendancePIN>
                        <PayCode xsi:nil="true" />
                      </Caregiver>
                      <ScheduleStartTime>14:00</ScheduleStartTime>
                      <ScheduleEndTime>18:00</ScheduleEndTime>
                      <VisitStartTime>14:05</VisitStartTime>
                      <VisitEndTime>18:10</VisitEndTime>
                      <EVVStartTime>14:07</EVVStartTime>
                      <EVVEndTime>18:08</EVVEndTime>
                      <IsMissedVisit>No</IsMissedVisit>
                      <TTOT>
                        <Hours>4</Hours>
                        <Minutes>10</Minutes>
                      </TTOT>
                      <Verification>
                        <VerifiedBy>Supervisor</VerifiedBy>
                        <Notes>Client requested additional assistance</Notes>
                        <VerifiedDate>2025-02-28T19:00:00Z</VerifiedDate>
                        <VerifiedTime>19:00</VerifiedTime>
                        <SupervisorName>Daniel Wilson</SupervisorName>
                      </Verification>
                      <Timesheet>
                        <Required>Yes</Required>
                        <Approved>Yes</Approved>
                      </Timesheet>
                      <TaskPerformed>
                        <POCTaskCode>102</POCTaskCode>
                        <AdditionalValue>2.0</AdditionalValue>
                        <Status>Completed</Status>
                        <Category>Mobility Assistance</Category>
                        <Duty>Wheelchair Support</Duty>
                        <Minutes>45</Minutes>
                      </TaskPerformed>
                      <VisitType>Follow-up</VisitType>
                      <ClockInEvvType>Telephony</ClockInEvvType>
                      <ClockOutEvvType>Telephony</ClockOutEvvType>
                      <ScheduleDuration>
                        <ScheduleDurationHours>4</ScheduleDurationHours>
                        <ScheduleDurationMinutes>0</ScheduleDurationMinutes>
                      </ScheduleDuration>
                      <BilledAmount>160.00</BilledAmount>
                      <BudgetNumber>5002</BudgetNumber>
                      <ActualHours>4.17</ActualHours>
                      <ActualHoursRounded>4</ActualHoursRounded>
                      <PayHours>4.17</PayHours>
                      <PayHoursUnrounded>4.17</PayHoursUnrounded>
                      <AdjustedHours>4</AdjustedHours>
                      <TimeZone>PST</TimeZone>
                    </GetVisitChangesV2Info>
                  </GetVisitChangesV2Result>
                </GetVisitChangesV2Response>
              </soap:Body>
            </soap:Envelope>
            """;

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(WrapperName = "GetVisitChangesV2", WrapperNamespace = "https://www.hhaexchange.com/apis/hhaws.integration", IsWrapped = true)]
        public partial class GetVisitChangesV2Request
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration", Order = 0)]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public AppParams Authentication;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration", Order = 1)]
            public System.DateTime ModifiedAfter;

            public GetVisitChangesV2Request()
            {
            }

            public GetVisitChangesV2Request(AppParams Authentication, System.DateTime ModifiedAfter)
            {
                this.Authentication = Authentication;
                this.ModifiedAfter = ModifiedAfter;
            }
        }


        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class AppParams
        {

            private string appNameField;

            private string appSecretField;

            private string appKeyField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public string AppName
            {
                get
                {
                    return this.appNameField;
                }
                set
                {
                    this.appNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string AppSecret
            {
                get
                {
                    return this.appSecretField;
                }
                set
                {
                    this.appSecretField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
            public string AppKey
            {
                get
                {
                    return this.appKeyField;
                }
                set
                {
                    this.appKeyField = value;
                }
            }
        }


        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class GetVisitChangesDetails
        {

            private int visitIDField;

            private string lastModifiedDateField;

            private int officeIDField;

            private string visitDateField;

            private PatientV2 patientField;

            private CaregiverV2 caregiverField;

            private string scheduleStartTimeField;

            private string scheduleEndTimeField;

            private string visitStartTimeField;

            private string visitEndTimeField;

            private string eVVStartTimeField;

            private string eVVEndTimeField;

            private string isMissedVisitField;

            private Ttotv2 tTOTField;

            private VerificationV2 verificationField;

            private TimesheetV2 timesheetField;

            private TaskV2[] taskPerformedField;

            private string visitTypeField;

            private ScheduleDuration scheduleDurationField;

            private string clockInEvvTypeField;

            private string clockOutEvvTypeField;

            private string billedAmountField;

            private System.Nullable<int> budgetNumberField;

            private string actualHoursField;

            private string actualHoursRoundedField;

            private string payHoursField;

            private string payHoursUnroundedField;

            private string adjustedHoursField;

            private string timeZoneField;

            private long splitVisitIDField;

            private long originalVisitIDField;

            private string suggestedStartTimeField;

            private string suggestedEndTimeField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int VisitID
            {
                get
                {
                    return this.visitIDField;
                }
                set
                {
                    this.visitIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string LastModifiedDate
            {
                get
                {
                    return this.lastModifiedDateField;
                }
                set
                {
                    this.lastModifiedDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
            public int OfficeID
            {
                get
                {
                    return this.officeIDField;
                }
                set
                {
                    this.officeIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
            public string VisitDate
            {
                get
                {
                    return this.visitDateField;
                }
                set
                {
                    this.visitDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
            public PatientV2 Patient
            {
                get
                {
                    return this.patientField;
                }
                set
                {
                    this.patientField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
            public CaregiverV2 Caregiver
            {
                get
                {
                    return this.caregiverField;
                }
                set
                {
                    this.caregiverField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
            public string ScheduleStartTime
            {
                get
                {
                    return this.scheduleStartTimeField;
                }
                set
                {
                    this.scheduleStartTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
            public string ScheduleEndTime
            {
                get
                {
                    return this.scheduleEndTimeField;
                }
                set
                {
                    this.scheduleEndTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 8)]
            public string VisitStartTime
            {
                get
                {
                    return this.visitStartTimeField;
                }
                set
                {
                    this.visitStartTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 9)]
            public string VisitEndTime
            {
                get
                {
                    return this.visitEndTimeField;
                }
                set
                {
                    this.visitEndTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 10)]
            public string EVVStartTime
            {
                get
                {
                    return this.eVVStartTimeField;
                }
                set
                {
                    this.eVVStartTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 11)]
            public string EVVEndTime
            {
                get
                {
                    return this.eVVEndTimeField;
                }
                set
                {
                    this.eVVEndTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 12)]
            public string IsMissedVisit
            {
                get
                {
                    return this.isMissedVisitField;
                }
                set
                {
                    this.isMissedVisitField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 13)]
            public Ttotv2 TTOT
            {
                get
                {
                    return this.tTOTField;
                }
                set
                {
                    this.tTOTField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 14)]
            public VerificationV2 Verification
            {
                get
                {
                    return this.verificationField;
                }
                set
                {
                    this.verificationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 15)]
            public TimesheetV2 Timesheet
            {
                get
                {
                    return this.timesheetField;
                }
                set
                {
                    this.timesheetField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("TaskPerformed", Order = 16)]
            public TaskV2[] TaskPerformed
            {
                get
                {
                    return this.taskPerformedField;
                }
                set
                {
                    this.taskPerformedField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 17)]
            public string VisitType
            {
                get
                {
                    return this.visitTypeField;
                }
                set
                {
                    this.visitTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 18)]
            public ScheduleDuration ScheduleDuration
            {
                get
                {
                    return this.scheduleDurationField;
                }
                set
                {
                    this.scheduleDurationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 19)]
            public string ClockInEvvType
            {
                get
                {
                    return this.clockInEvvTypeField;
                }
                set
                {
                    this.clockInEvvTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 20)]
            public string ClockOutEvvType
            {
                get
                {
                    return this.clockOutEvvTypeField;
                }
                set
                {
                    this.clockOutEvvTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 21)]
            public string BilledAmount
            {
                get
                {
                    return this.billedAmountField;
                }
                set
                {
                    this.billedAmountField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 22)]
            public System.Nullable<int> BudgetNumber
            {
                get
                {
                    return this.budgetNumberField;
                }
                set
                {
                    this.budgetNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 23)]
            public string ActualHours
            {
                get
                {
                    return this.actualHoursField;
                }
                set
                {
                    this.actualHoursField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 24)]
            public string ActualHoursRounded
            {
                get
                {
                    return this.actualHoursRoundedField;
                }
                set
                {
                    this.actualHoursRoundedField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 25)]
            public string PayHours
            {
                get
                {
                    return this.payHoursField;
                }
                set
                {
                    this.payHoursField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 26)]
            public string PayHoursUnrounded
            {
                get
                {
                    return this.payHoursUnroundedField;
                }
                set
                {
                    this.payHoursUnroundedField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 27)]
            public string AdjustedHours
            {
                get
                {
                    return this.adjustedHoursField;
                }
                set
                {
                    this.adjustedHoursField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 28)]
            public string TimeZone
            {
                get
                {
                    return this.timeZoneField;
                }
                set
                {
                    this.timeZoneField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 29)]
            public long SplitVisitID
            {
                get
                {
                    return this.splitVisitIDField;
                }
                set
                {
                    this.splitVisitIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 30)]
            public long OriginalVisitID
            {
                get
                {
                    return this.originalVisitIDField;
                }
                set
                {
                    this.originalVisitIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 31)]
            public string SuggestedStartTime
            {
                get
                {
                    return this.suggestedStartTimeField;
                }
                set
                {
                    this.suggestedStartTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 32)]
            public string SuggestedEndTime
            {
                get
                {
                    return this.suggestedEndTimeField;
                }
                set
                {
                    this.suggestedEndTimeField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class CaregiverV2
        {

            private int idField;

            private string firstNameField;

            private string lastNameField;

            private string caregiverCodeField;

            private int timeAndAttendancePINField;

            private PayCode payCodeField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int ID
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string FirstName
            {
                get
                {
                    return this.firstNameField;
                }
                set
                {
                    this.firstNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
            public string LastName
            {
                get
                {
                    return this.lastNameField;
                }
                set
                {
                    this.lastNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
            public string CaregiverCode
            {
                get
                {
                    return this.caregiverCodeField;
                }
                set
                {
                    this.caregiverCodeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
            public int TimeAndAttendancePIN
            {
                get
                {
                    return this.timeAndAttendancePINField;
                }
                set
                {
                    this.timeAndAttendancePINField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
            public PayCode PayCode
            {
                get
                {
                    return this.payCodeField;
                }
                set
                {
                    this.payCodeField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class PatientV2
        {

            private int idField;

            private string admissionNumberField;

            private string firstNameField;

            private string lastNameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int ID
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string AdmissionNumber
            {
                get
                {
                    return this.admissionNumberField;
                }
                set
                {
                    this.admissionNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
            public string FirstName
            {
                get
                {
                    return this.firstNameField;
                }
                set
                {
                    this.firstNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
            public string LastName
            {
                get
                {
                    return this.lastNameField;
                }
                set
                {
                    this.lastNameField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class Ttotv2
        {

            private int hoursField;

            private int minutesField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int Hours
            {
                get
                {
                    return this.hoursField;
                }
                set
                {
                    this.hoursField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public int Minutes
            {
                get
                {
                    return this.minutesField;
                }
                set
                {
                    this.minutesField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class VerificationV2
        {

            private string verifiedByField;

            private string notesField;

            private System.DateTime verifiedDateField;

            private string verifiedTimeField;

            private string supervisorNameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public string VerifiedBy
            {
                get
                {
                    return this.verifiedByField;
                }
                set
                {
                    this.verifiedByField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string Notes
            {
                get
                {
                    return this.notesField;
                }
                set
                {
                    this.notesField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
            public System.DateTime VerifiedDate
            {
                get
                {
                    return this.verifiedDateField;
                }
                set
                {
                    this.verifiedDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
            public string VerifiedTime
            {
                get
                {
                    return this.verifiedTimeField;
                }
                set
                {
                    this.verifiedTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
            public string SupervisorName
            {
                get
                {
                    return this.supervisorNameField;
                }
                set
                {
                    this.supervisorNameField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class TimesheetV2
        {

            private string requiredField;

            private string approvedField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public string Required
            {
                get
                {
                    return this.requiredField;
                }
                set
                {
                    this.requiredField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string Approved
            {
                get
                {
                    return this.approvedField;
                }
                set
                {
                    this.approvedField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class TaskV2
        {

            private int pOCTaskCodeField;

            private decimal additionalValueField;

            private string statusField;

            private string categoryField;

            private string dutyField;

            private int minutesField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int POCTaskCode
            {
                get
                {
                    return this.pOCTaskCodeField;
                }
                set
                {
                    this.pOCTaskCodeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public decimal AdditionalValue
            {
                get
                {
                    return this.additionalValueField;
                }
                set
                {
                    this.additionalValueField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
            public string Status
            {
                get
                {
                    return this.statusField;
                }
                set
                {
                    this.statusField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
            public string Category
            {
                get
                {
                    return this.categoryField;
                }
                set
                {
                    this.categoryField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
            public string Duty
            {
                get
                {
                    return this.dutyField;
                }
                set
                {
                    this.dutyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
            public int Minutes
            {
                get
                {
                    return this.minutesField;
                }
                set
                {
                    this.minutesField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class ScheduleDuration
        {

            private int scheduleDurationHoursField;

            private int scheduleDurationMinutesField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int ScheduleDurationHours
            {
                get
                {
                    return this.scheduleDurationHoursField;
                }
                set
                {
                    this.scheduleDurationHoursField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public int ScheduleDurationMinutes
            {
                get
                {
                    return this.scheduleDurationMinutesField;
                }
                set
                {
                    this.scheduleDurationMinutesField = value;
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "https://www.hhaexchange.com/apis/hhaws.integration")]
        public partial class PayCode
        {

            private int idField;

            private string nameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
            public int ID
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
