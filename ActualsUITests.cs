using Controcc.Web.UITesting.PageObjects.ControccWebTestBaseClasses;
using FluentAssertions;
using OCC.UI.TestingFramework.WebDriverExtensions;
using System;
using Xunit;
using xRetry;
using Controcc.Web.UITesting.PageObjects.Actuals;
using Controcc.Web.UITesting.PageObjects.Actuals.Modals;
using static Controcc.Web.UITesting.PageObjects.Elements.CWebListElement;

namespace Controcc.Web.UITesting.Tests.UITests
{
	public class ActualsUITests : ControccWebAutomatedTestBase, IDisposable
	{
		[RetryTheory(typeof(SkipException))]
		[MemberData(nameof(Browsers))]
		public void AddNewActual(BrowserEnum browser)
		{
			StartTest(browser, () =>
			{
				ActualsScreen actualsScreen = StartPage
									.PaymentsHomeNavigator()
									.GoToActuals();
				AddEditActualsModalFromActualsScreen addActual = actualsScreen.ActualsList.PressButton<AddEditActualsModalFromActualsScreen>("Add");
				addActual.Provider.SearchAndSelectByText("Helper");
				addActual.ServiceLevel.SetValue("DOMCAREN Domestic Care BAND2 WEEKEND (30 MINUTES) (30 Minutes)");
				addActual.Client.SearchAndSelectByText("SSR 23");
				addActual.Dates.SelectNthOption(3);
				addActual.Frustrated.SetValue("1");
				addActual.Missed.SetValue("2");
				addActual.Extra.SetValue("4");
				addActual.Visits.SetValue("5");
				addActual.Comments.SetValue("Adding actual during automated regression checks");
				addActual.SaveButton.Click();
				actualsScreen.ServiceFilter.SearchAndSelectByText("Home Care (Helper)");
				actualsScreen.ClientFilter.SearchAndSelectByText("SSR 23");
				actualsScreen.FromDate.SetValue(new DateTime(2009, 01, 01));
				actualsScreen.ToDate.SetValue(new DateTime(2009, 03, 31));
				actualsScreen.ApplyButton.Click();
				actualsScreen.ActualsList.ResizeListColumns("Fit to available space");
				actualsScreen.ActualsList.GetCellValue(0, "actualQuantityF").Should().Be("5");
				actualsScreen.ActualsList.GetCellValue(0, "differences").Should().Be("2M, 1F, 4E");
				actualsScreen.ActualsList.GetCellValue(0, "cost").Should().Be("£66.00");
				actualsScreen.ActualsList.SelectRow(0);
				actualsScreen.Comments.GetText().Should().Be("Adding actual during automated regression checks");
			});
		}

		[RetryTheory(typeof(SkipException))]
		[MemberData(nameof(Browsers))]
		public void GenerateAndConfirmActuals(BrowserEnum browser)
		{
			StartTest(browser, () =>
			{
				ActualsScreen actualsScreen = NavigateToRelativeUrl<ActualsScreen>("Actuals");
				GenerateActualsModal generateActuals = actualsScreen.ActualsList.PressButton<GenerateActualsModal>("Generate");
				generateActuals.Provider.SearchAndSelectByText("Hilltop");
				generateActuals.Contract.SearchAndSelectByText("Hilltop");
				generateActuals.Service.SearchAndSelectByText("Day Care");
				generateActuals.To.SetValue("09/03/2009 - 15/03/2009");
				generateActuals.GenerateButton.Click();
				actualsScreen.ProviderFilter.SearchAndSelectByText("Hilltop");
				actualsScreen.GeneratedCheckbox.SetTriStateValue("true");
				actualsScreen.FromDate.SetValue(new DateTime(2009, 02, 09));
				actualsScreen.ApplyButton.Click();
				actualsScreen.ActualsList.ListFilter("ssRef", "filterLabelSS ref", "12");
				actualsScreen.ActualsList.CtrlSelectMultipleRowsWithGivenValue("ssRef", "SSR 12");
				actualsScreen.ActualsList.PressButton("Confirm generated");
				actualsScreen.ClientFilter.SearchAndSelectByText("Ko");
				actualsScreen.GeneratedCheckbox.SetTriStateValue("false");
				actualsScreen.ApplyButton.Click();
				BulkEditActualsModal bulkEditActuals = actualsScreen.ActualsList.PressButton<BulkEditActualsModal>("Edit bulk");
				bulkEditActuals.ActualDeliveryList.SetCellValue(0, "extraQuantity", EditorTypeEnum.text, "2");
				bulkEditActuals.ActualDeliveryList.SetCellValue(1, "missedQuantity", EditorTypeEnum.text, "1");
				bulkEditActuals.ActualDeliveryList.SetCellValue(1, "extraQuantity", EditorTypeEnum.text, "1");
				bulkEditActuals.ActualDeliveryList.SetCellValue(2, "abortedQuantity", EditorTypeEnum.text, "2");
				bulkEditActuals.SaveButton.Click();
				actualsScreen.ActualsList.SelectRowByCellValue("startDate", "09/02/2009");
				ActualsDeletionConfirmation confirmDeletion = actualsScreen.ActualsList.PressButton<ActualsDeletionConfirmation>("Delete");
				confirmDeletion.AcceptConfirmation();
				actualsScreen.ActualsList.GetRowCount().Should().Be(4);
				BulkEditActualsModal bulkEditActuals2 = actualsScreen.ActualsList.PressButton<BulkEditActualsModal>("Edit bulk");
				bulkEditActuals2.ActualDeliveryList.GetCellValue(0, "actualQuantity").Should().Be("4");
				bulkEditActuals2.ActualDeliveryList.GetCellValue(1, "actualQuantity").Should().Be("2");
				bulkEditActuals2.ActualDeliveryList.GetCellValue(2, "actualQuantity").Should().Be("0");
				bulkEditActuals2.ActualDeliveryList.GetRowCount().Should().Be(4);
			});
		}

		[RetryTheory(typeof(SkipException))]
		[MemberData(nameof(Browsers))]
		public void AddDispute(BrowserEnum browser)
		{
			StartTest(browser, () =>
			{
				ActualsScreen actualsScreen = StartPage
									.PaymentsHomeNavigator()
									.GoToActuals();
				actualsScreen.ClientFilter.SearchAndSelectByText("SSR 2");
				actualsScreen.ApplyButton.Click();
				actualsScreen.ActualsList.SelectRowByCellValue("actualQuantityF", "7");
				CreateDisputeModal createDispute = actualsScreen.ActualsList.PressButton<CreateDisputeModal>("Create dispute");
				createDispute.Reason.SetValue("Disputed");
				createDispute.Details.SetValue("Details of the disputed actual are entered here.");
				createDispute.SaveButton.Click();
				actualsScreen.ProviderFilter.SearchAndSelectByText("Great Northern");
				actualsScreen.ApplyButton.Click();
				actualsScreen.ActualsList.SelectRowByCellValue("startDate", "09/02/2009");
				actualsScreen.ExceptionsWarning.GetText().Should().Be("This actual has one or more exceptions.");
				actualsScreen.DisputeTextAfterException.GetText().Should().Be("This actual is currently in dispute.");
			});
		}

		[RetryTheory(typeof(SkipException))]
		[MemberData(nameof(Browsers))]
		public void ViewMissingActualsReport(BrowserEnum browser)
		{
			StartTest(browser, () =>
			{
				ActualsScreen actualsScreen = NavigateToRelativeUrl<ActualsScreen>("Actuals");
				MissingActualsReport missingActuals = actualsScreen.ActualsList.PressButton<MissingActualsReport>("Missing actuals report");
				missingActuals.ReportGeneratedSuccessfully().Should().BeTrue();
			});
		}

		[RetryTheory(typeof(SkipException))]
		[MemberData(nameof(Browsers))]
		public void EditTimetabledActuals(BrowserEnum browser)
		{
			StartTest(browser, () =>
			{
				ActualsScreen actualsScreen = StartPage
									.PaymentsHomeNavigator()
									.GoToActuals();
				actualsScreen.ClientFilter.SearchAndSelectByText("Timberlake");
				actualsScreen.ApplyButton.Click();
				actualsScreen.ActualsList.SelectRowByCellValue("startDate", "09/02/2009");
				AddEditTimetabledActuals editActuals = actualsScreen.ActualsList.PressButton<AddEditTimetabledActuals>("Edit");
				editActuals.WeeklyTimetabledActuals.SetCellValue(0, "deliveredOnFriday", EditorTypeEnum.checkbox, "", false);
				editActuals.WeeklyTimetabledActuals.SetCellValue(1, "deliveredOnThursday", EditorTypeEnum.checkbox, "", false);
				editActuals.WeeklyTimetabledActuals.PressButton("Add");
				int latestRowNumber = editActuals.WeeklyTimetabledActuals.GetRowCount() - 1;
				editActuals.WeeklyTimetabledActuals.SetCellValue(latestRowNumber, "deliveredOnMonday", EditorTypeEnum.checkbox, "", false);
				editActuals.WeeklyTimetabledActuals.SetCellValue(latestRowNumber, "deliveredOnTuesday", EditorTypeEnum.checkbox, "", false);
				editActuals.WeeklyTimetabledActuals.SetCellValue(latestRowNumber, "deliveredOnSaturday", EditorTypeEnum.checkbox, "", false);
				editActuals.WeeklyTimetabledActuals.SetTimepickerCellValue(latestRowNumber, "entryTime", "20:00", "Entry");
				editActuals.WeeklyTimetabledActuals.SetTimepickerCellValue(latestRowNumber, "exitTime", "20:45", "Exit");
				editActuals.FrustratedActuals.PressButton("Add");
				latestRowNumber = editActuals.FrustratedActuals.GetRowCount() - 1;
				editActuals.FrustratedActuals.SetCellValue(latestRowNumber, "deliveredOnMonday", EditorTypeEnum.checkbox, "", false);
				editActuals.FrustratedActuals.SetTimepickerCellValue(latestRowNumber, "entryTime", "20:00", "Entry");
				editActuals.FrustratedActuals.SetTimepickerCellValue(latestRowNumber, "exitTime", "20:59", "Exit");
				editActuals.FrustratedActuals.SetCellValue(latestRowNumber, "comments", EditorTypeEnum.text, "Client didn't answer the door");
				editActuals.SaveButton.Click();
				actualsScreen.ActualsList.GetCellValue(0, "actualQuantityF").Should().Be("15.25");
				actualsScreen.ActualsList.GetCellValue(0, "differences").Should().Be("1F, 4.5E");
			});
		}
	}
}
