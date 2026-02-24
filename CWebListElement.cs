using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.PageObject.Common.Elements;
using OCC.UI.TestingFramework.WebDriverExtensions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Controcc.Web.UITesting.PageObjects.Elements
{
	[Serializable]
	public class CWebListElement : PageElementBase
	{
		public enum EditorTypeEnum
		{
			text,
			money,
			date,
			dropdown,
			numeric,
			checkbox,
			multicolumndropdown,
			button
		}

		private Dictionary<string, IWebElement> buttons;

		public CWebListElement(PageObjectBase page, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(page, inputSelector, selectBy)
		{
		}



		#region public methods

		public void PressButton(string label)
		{
			WaitForLoadingScreenToClose();
			getButton(label).Click();
		}

		public void PressSubMenuButton(string modalPrefix, int subMenuNumber, string value)
		{
			var div = OwnerPage.Driver.FindElement(GetBy());
			var showMore = div.FindElement(By.CssSelector($".slds-button-group:nth-of-type({subMenuNumber}) [data-uitesting-id='{modalPrefix}Show more']"));
			showMore.Click();

			var valueDisplay = div.FindElement(By.CssSelector($"[data-uitesting-id='dropdown-{modalPrefix}{value}']"));
			valueDisplay.Click();
		}


		public TPage PressSubMenuButton<TPage>(string modalPrefix, int subMenuNumber, string value) where TPage : PageObjectBase, new()
		{
			var div = OwnerPage.Driver.FindElement(GetBy());
			var showMore = div.FindElement(By.CssSelector($".slds-button-group:nth-of-type({subMenuNumber}) [data-uitesting-id='{modalPrefix}Show more']"));
			showMore.Click();

			var valueDisplay = div.FindElement(By.CssSelector($"[data-uitesting-id='dropdown-{modalPrefix}{value}']"));
			valueDisplay.Click();

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public TPage PressButton<TPage>(string label) where TPage : PageObjectBase, new()
		{
			WaitForLoadingScreenToClose();
			PressButton(label);

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public TPage ChooseFromMultipleOptions<TPage>(string label, string option) where TPage : PageObjectBase, new()
		{
			IWebElement defaultButton = getButton(label);
			var alternativeOptionSelector = defaultButton.FindElement(By.XPath(".//../div/button[@data-uitesting-id='Show more']"));
			alternativeOptionSelector.Click();
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var alternativeOption = wrapper.FindElement(By.XPath($"//li/a/span[contains(text(), '{option}')]"));
			alternativeOption.Click();
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public void ChooseFromMultipleOptions(string label, string option)
		{
			IWebElement defaultButton = getButton(label);
			var alternativeOptionSelector = defaultButton.FindElement(By.XPath(".//../div/button[@data-uitesting-id='Show more']"));
			alternativeOptionSelector.Click();
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var alternativeOption = wrapper.FindElement(By.XPath($"//li/a/span[contains(text(), '{option}')]"));
			alternativeOption.Click();
		}

		public bool ButtonEnabled(string label)
		{
			var btn = getButton(label);
			return btn != null ? !btn.IsReadOnly() : false;
		}

		public void CheckFirstColumnCheckbox(int rowIndex)
		{
			var row = getRow(rowIndex);
			var cells = row.FindElements(By.ClassName("ag-cell-value"));

			foreach (var cell in cells)
			{
				var colID = cell.GetAttribute("col-id");
				if (colID != null && colID.Equals("__rowChecked__"))
				{
					cell.Click();
					break;
				}
			}
		}

		public bool CheckboxChecked(int rowIndex, string label)
		{
			var checkboxCell = GetCell(rowIndex, label);
			var checkbox = checkboxCell.FindElement(By.TagName("input"));
			if (checkbox == null)
				throw new ArgumentException($"{label} does not have an input control");
			return checkbox.GetAttribute("checked") == "true";
		}

		public void SelectRow(int index)
		{
			WaitForLoadingScreenToClose();
			var row = getRow(index);
			row.Click();
		}

		public void CtrlSelectRow(int index, string headerName)
		{
			var cell = GetCell(index, headerName);
			OpenQA.Selenium.Interactions.Actions actions = new OpenQA.Selenium.Interactions.Actions(OwnerPage.Driver.DecoratedWebDriver);
			actions.KeyDown(Keys.LeftControl).Click(cell).KeyUp(Keys.LeftControl).Build().Perform();
		}

		public TPage SelectRowByCellValue<TPage>(string headerName, string value) where TPage : PageObjectBase, new()
		{
			SelectRowByCellValue(headerName, value);
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public void SelectRowByCellValue(string headerName, string value)
		{
			WaitForLoadingScreenToClose();
			var cellElement = getCellByColumnAndValue(headerName, value);
			cellElement.Click();
		}

		public void SelectSelectableRow(int rowIndex)
		{
			var row = getRow(rowIndex);
			var cell = row.FindElement(By.XPath("./div[1]"));
			var input = cell.FindElement(By.TagName("input"));
			input.Click();
		}

		public int GetRowIndexOfKnownCell(string headerName, string value)
		{
			int rowCount = GetRowCount();
			for (int i = 0; i < rowCount; i++)
			{
				if (GetCellValue(i, headerName) == value)
				{
					return i;
				}
			}
			return -1;
		}

		public void SelectMultipleRowsWithGivenValue(string headerName, string value)
		{
			int count = GetRowCount();
			for (int i = 0; i < count; i++)
			{
				if (GetCellValue(i, headerName) == value)
				{
					SelectRow(i);
				}
			}
		}

		public void CtrlSelectMultipleRowsWithGivenValue(string headerName, string value)//select multiple rows at the same time
		{
			int count = GetRowCount();
			SelectRowByCellValue(headerName, value);
			for (int i = 1; i < count; i++)
			{
				if (GetCellValue(i, headerName) == value)
				{
					CtrlSelectRow(i, headerName);
				}
			}
		}

		public void SelectDeletedRow()
		{
			IWebElement deletedRow = getDeletedRow();
			deletedRow.Click();
		}

		public bool RowIsPresentInList(string headerName, string value)
		{
			try
			{
				SelectRowByCellValue(headerName, value);
				return true;
			}
			catch (Exception)
			{
			}
			return false;
		}

		public void SetCheckboxValue(string checkboxLabel, bool value)
		{
			var div = OwnerPage.Driver.FindElement(GetBy());
			var element = div.FindElement(By.CssSelector($"[data-uitesting-id='{checkboxLabel}']"));
			var inputElement = element.FindElement(By.TagName("input"));
			var labelElement = element.FindElement(By.TagName("label"));			

			if (inputElement.Selected != value)
			{
				WaitForLoadingScreenToClose();
				labelElement.Click();
			}
		}

		public void SetCellValue(int rowIndex, string columnName, EditorTypeEnum editorType, string value, bool doubleClick = true, string extraDetails = "")
		{
			var row = getRow(rowIndex);
			var cells = row.FindElements(By.ClassName("ag-cell-value"));
			foreach (var cell in cells)
			{
				var colID = cell.GetAttribute("col-id");
				if (colID != null
					&& colID.ToLower().Split("_")[0].Replace(" ", "").Equals(columnName.ToLower().Replace(" ", ""))
					)
				{
					if (doubleClick)
					{
						DoubleClick(cell);
					}

					try
					{
						IWebElement editor;
						switch (editorType)
						{
							case EditorTypeEnum.text:
								editor = OwnerPage.Driver.FindElement(By.CssSelector(".ag-cell-focus input"));
								editor.Clear();
								editor.SendKeys(value);
								editor.SendKeys(Keys.Enter);
								break;
							case EditorTypeEnum.money:
								editor = OwnerPage.Driver.FindElement(By.Id("moneyCellEditor"));
								editor.Clear();
								editor.SendKeys(value);
								editor.SendKeys(Keys.Enter);
								break;
							case EditorTypeEnum.date:
								editor = OwnerPage.Driver.FindElement(By.CssSelector($"[data-uitesting-id='{extraDetails}Pick a date']"));
								try
								{
									IWebElement clearButton = editor.FindElement(By.XPath("./parent::*/button"));
									clearButton.Click();
									DoubleClick(cell);
									IWebElement editor2 = OwnerPage.Driver.FindElement(By.CssSelector($"[data-uitesting-id='{extraDetails}Pick a date']"));
									editor2.Click();
									editor2.SendKeys(value);
									editor2.SendKeys(Keys.Enter);
								}
								catch (NoSuchElementException)
								{
									editor.Click();
									editor.SendKeys(value);
									editor.SendKeys(Keys.Enter);
								}
								break;
							case EditorTypeEnum.numeric:
								editor = OwnerPage.Driver.FindElement(By.Id("numericCellEditor"));
								editor.Clear();
								editor.SendKeys(value);
								editor.SendKeys(Keys.Enter);
								break;
							case EditorTypeEnum.button:
								editor = cell.FindElement(By.TagName("button"));
								editor.Click();
								break;
							case EditorTypeEnum.dropdown:
								editor = OwnerPage.Driver.FindElement(By.ClassName("ag-popup-editor"));
								var opts = editor.FindElements(By.TagName("div"));

								foreach (var opt in opts)
								{
									try
									{

										if (opt.Text.Equals(value))
										{
											var a = opt.Text;
											opt.Click();
										}
									}
									catch (Exception)
									{
									}
								}
								break;
							case EditorTypeEnum.multicolumndropdown:
								editor = OwnerPage.Driver.FindElement(By.ClassName("ag-popup-editor"));
								var option = editor.FindElement(By.TagName("input"));
								option.SendKeys(value);
								option.SendKeys(Keys.Enter);
								break;
							case EditorTypeEnum.checkbox:
								editor = cell.FindElement(By.TagName("input"));
								editor.Click();
								break;
						}
					}
					catch (Exception)
					{
					}

					break; //get out of foreach
				}
			}
		}

		public void SetTimepickerCellValue(int rowIndex, string columnName, string value, string ariaLabel)
		{
			var row = getRow(rowIndex);
			var cells = row.FindElements(By.ClassName("ag-cell-value"));
			foreach (var cell in cells)
			{
				var colID = cell.GetAttribute("col-id");
				if (colID != null
					&& colID.ToLower().Split("_")[0].Replace(" ", "").Equals(columnName.ToLower().Replace(" ", ""))
					)
				{
					OpenQA.Selenium.Interactions.Actions actions = new OpenQA.Selenium.Interactions.Actions(OwnerPage.Driver.DecoratedWebDriver);
					actions.MoveToElement(cell, 20, 10).DoubleClick().Perform();
					var editor = OwnerPage.Driver.FindElement(By.CssSelector($"[aria-label='{ariaLabel}']"));
					editor.SendKeys(value);	
				}
			}
		}

		public TPage SetCellValue<TPage>(int rowIndex, string columnName, EditorTypeEnum editorType, string value, bool doubleClick = true) where TPage : PageObjectBase, new()
		{
			SetCellValue(rowIndex, columnName, editorType, value, doubleClick);
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public string GetCellValue(int rowIndex, string columnName)
		{
			WaitForLoadingScreenToClose();
			var row = getRow(rowIndex);
			var cells = row.FindElements(By.ClassName("ag-cell-value"));

			foreach (var cell in cells)
			{
				var colID = cell.GetAttribute("col-id");
				if (colID != null && colID.ToLower().Split("_")[0].Replace(" ", "").Equals(columnName.ToLower().Replace(" ", "")))
				{
					return cell.Text;
				}
			}

			return null;
		}


		public void DoubleClickRow(int index)
		{
			var row = getRow(index);
			DoubleClick(row);
		}

		public TPage DoubleClickRow<TPage>(int index) where TPage : PageObjectBase, new()
		{
			DoubleClickRow(index);

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public void DoubleClickCell(string headerName, string value)
		{
			var cell = getCellByColumnAndValue(headerName, value);
			DoubleClick(cell);
		}

		public void DoubleClickCellInRow(int index, string headerName)
		{
			var cell = GetCell(index, headerName);
			DoubleClick(cell);
		}

		public TPage DoubleClickCell<TPage>(string headerName, string value) where TPage : PageObjectBase, new()
		{
			DoubleClickCell(headerName, value);

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public TPage DoubleClickCellInRow<TPage>(int index, string headerName) where TPage : PageObjectBase, new()
		{
			DoubleClickCellInRow(index, headerName);

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public int GetRowCount()
		{
			try
			{
				var wrapper = OwnerPage.Driver.FindElement(GetBy());
				var bodyList = wrapper.FindElement(By.ClassName("ag-center-cols-container"));
				var rows = bodyList.FindElements(By.TagName("div"));

				int count = 0;

				foreach (var row in rows)
				{
					var role = row.GetAttribute("role");
					if (role != null && role.Equals("row"))
					{
						count++;
					}
				}

				return count;
			}
			catch (Exception) { }

			return 0;
		}

		public void ListFilter(string header, string filterBoxName, string filterText)
		{
			WaitForLoadingScreenToClose();
			var element = getHeader(header);
			var filterButton = element.FindElement(By.CssSelector(".slds-button.slds-button_icon.list__control-button:nth-of-type(2)"));
			filterButton.Click();

			var div = OwnerPage.Driver.FindElement(GetBy());
			var input = div.FindElement(By.Id(filterBoxName));
			input.Clear();
			input.SendKeys(filterText);
			input.SendKeys(Keys.Enter);
		}

		public void DownloadListContentsAsCSV()
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var downloadButton = wrapper.FindElement(By.CssSelector("[data-uitesting-id='Download list as CSV file']"));
			downloadButton.Click();
		}

		public void ResizeListColumns(string option)
		{
			WaitForLoadingScreenToClose();
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var resizeButton = wrapper.FindElement(By.CssSelector("button[title='Resize columns']"));
			resizeButton.Click();
			var resizeAction = wrapper.FindElement(By.CssSelector($"a[data-uitesting-id='dropdown-{option}'"));
			resizeAction.Click();
		}

		public TPage ClickOnLink<TPage>(string headerName, string value) where TPage : PageObjectBase, new()
		{
			var cell = getCellByColumnAndValue(headerName, value);
			var link = cell.FindElement(By.TagName("a"));

			link.Click();

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public string GetEmptyListText()
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var overlay = wrapper.FindElement(By.CssSelector(".ag-overlay-wrapper"));
			string listText = overlay.Text;
			return listText;
		}

		public TPage GoToLinkedEntity<TPage>(string headerName, string value) where TPage : PageObjectBase, new()
		{
			WaitForLoadingScreenToClose();
			var cell = getCellByColumnAndValue(headerName, value);
			cell.Click();

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}

		public void RefreshPageIfListNotPopulated()
		{
			int i = 0;
			while (i < 10)
			{
				WaitForLoadingScreenToClose();
				int rowCount = GetRowCount();
				if (rowCount == 0)
				{
					Driver.Navigate().Refresh();
				}
				else
				{
					return;
				}
				i++;
			}
			throw new Exception("After 10 refreshes the list is still not populated - this suggests that something is wrong.");
		}

		public void RefreshPageIfRowNotPresent(string headerName, string value)
		{
			int i = 0;
			while (i < 10)
			{
				WaitForLoadingScreenToClose();
				bool rowPresent = RowIsPresentInList(headerName, value);
				if (!rowPresent)
				{
					Driver.Navigate().Refresh();
				}
				else
				{
					return;
				}
				i++;
			}
			throw new Exception("After 10 refreshes the list still doesn't contain the expected data - this suggests that something is wrong.");
		}

		#endregion public methods

		#region private methods
		private void fillButtons()
		{
			if (buttons == null)
				buttons = new Dictionary<string, IWebElement>();

			try
			{
				var wrapper = OwnerPage.Driver.FindElement(GetBy());
				var buttonElements = wrapper.FindElements(By.TagName("button"));
				foreach (var button in buttonElements)
				{
					if (!buttons.ContainsKey(button.Text))
					{
						buttons.Add(button.Text, button);
					}
				}
			}
			catch (Exception) { }
		}

		private IWebElement getButton(string label)
		{
			if (buttons == null || !buttons.ContainsKey(label))
			{
				fillButtons();
			}

			if (buttons.ContainsKey(label))
				return buttons[label];

			throw new ArgumentException($"The requested button (Label = {label}) does not exist for this list");
		}

		private IWebElement getRow(int index)
		{
			try
			{
				var wrapper = OwnerPage.Driver.FindElement(GetBy());
				var bodyList = wrapper.FindElement(By.ClassName("ag-center-cols-container"));
				var rows = bodyList.FindElements(By.TagName("div"));

				int i = 0;

				foreach (var row in rows)
				{
					var role = row.GetAttribute("role");
					if (role != null && role.Equals("row"))
					{
						if (i == index)
							return row;
						else
							i++;
					}
				}
			}
			catch (Exception) { }

			return null;
		}
		private IWebElement GetCell(int rowIndex, string columnName)
		{
			var row = getRow(rowIndex);
			var cells = row.FindElements(By.ClassName("ag-cell-value"));

			foreach (var cell in cells)
			{
				var colID = cell.GetAttribute("col-id");
				if (colID != null && colID.ToLower().Split("_")[0].Equals(columnName.ToLower().Replace(" ", "")))
				{
					return cell;
				}
			}

			throw new ArgumentException($"The column {columnName} does not exist in list");
		}

		private IWebElement getCellByColumnAndValue(string headerName, string value)
		{
			try
			{
				var wrapper = OwnerPage.Driver.FindElement(GetBy());
				var bodyList = wrapper.FindElement(By.ClassName("ag-center-cols-container"));
				var cells = bodyList.FindElements(By.ClassName("ag-cell-value"));

				foreach (var cell in cells)
				{
					var colID = cell.GetAttribute("col-id");
					if (colID != null
						&& colID.ToLower().Split("_")[0].Equals(headerName.ToLower().Replace(" ", ""))
						&& cell.Text.ToLower().Equals(value.ToLower())
						)
					{
						return cell;
					}
				}
			}
			catch (Exception) { }

			return null;
		}

		private IWebElement getHeader(string headerName)
		{
			try
			{
				var wrapper = OwnerPage.Driver.FindElement(GetBy());
				var headerRow = wrapper.FindElement(By.XPath("//div[@class='ag-header-row ag-header-row-column']"));
				var headers = headerRow.FindElements(By.TagName("div"));

				foreach (var header in headers)
				{
					var colID = header.GetAttribute("col-id");
					if (colID != null
						&& colID.ToLower().Split("_")[0].Equals(headerName.ToLower().Replace(" ", ""))
						)
					{
						return header;
					}
				}
			}
			catch (Exception) { }

			return null;
		}

		private IWebElement getDeletedRow()
		{
			try
			{
				var deletedRow = OwnerPage.Driver.FindElement(By.CssSelector("input[checked]"));
				return deletedRow;
			}
			catch (Exception) { }

			return null;
		}

		#endregion private methods
	}
}
