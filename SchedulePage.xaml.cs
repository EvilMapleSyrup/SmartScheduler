using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Text;
using System.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentDate;
using FluentDateTime;
using FluentDateTimeOffset;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI;


namespace SmartScheduler
{
    public sealed partial class SchedulePage : Page
    {
        public ObservableCollection<Worker> workers = new ObservableCollection<Worker>();
        ObservableCollection<string> existingPresets = new ObservableCollection<string>();
        Dictionary<string, int> confirmedChats = new Dictionary<string, int>();
        List<DateTime> currentDays = new List<DateTime>();
        int dtFontSize = 14;
        SaveLoadHandler saveLoad = new SaveLoadHandler(); //save and load handler for serializing the data
        TGramBot tgb = new TGramBot();
        //TimeSpan notificationTime = new TimeSpan();
        //bool notifOn = false;
        //DailyTrigger dTrigger; 

        public SchedulePage()
        {
            this.InitializeComponent();
            StartupLoad();
        }

        private void StartupLoad()
        {
            ///
            /// For loading variables on start. Sends the requested file name and load type to the handler.
            ///
            if (saveLoad.LoadHandler("temp", 'w') != null)
            {
                workers = saveLoad.LoadHandler("temp", 'w');
            }
            if(saveLoad.LoadHandler("dtFontSize", 'i') != null)
            {
                dtFontSize = saveLoad.LoadHandler("dtFontSize", 'i');
            }
            if (saveLoad.LoadHandler("presetList", 'p') != null)
            {
                existingPresets = saveLoad.LoadHandler("presetList", 'p');
            }
            //Notif save values, commented for now as it's not used
            /*if (saveLoad.LoadHandler("notifBool", 'b') != null)
            {
                notifOn = saveLoad.LoadHandler("notifBool", 'b');
            }
            if (saveLoad.LoadHandler("notifTime", 't') != null && notifOn == true)
            {
                notificationTime = saveLoad.LoadHandler("notifTime", 't');
                NotifHandler();
                NotifTextChanger();
            }*/
            if (saveLoad.LoadHandler("confirmedChats", 'd') != null)
            {
                confirmedChats = saveLoad.LoadHandler("confirmedChats", 'd');
            }
            
            SetScheduleDays(); // sets up the day number and date of the scheduler
            FontChangeHandler(false); // sets font to required size
            RefreshDT(); // refresh table to reflect changes
            
        }

        //logic to trigger the notifications daily from set time
        /*
        public void NotifHandler()
        {
            dTrigger = new DailyTrigger(notificationTime);

            dTrigger.OnTimeTriggered += () =>
            {
                NotifTextChanger();
                dTrigger.Repeat();
            };
            
        }*/

        private async void FontChangeHandler(bool isPrompt)
        {
            int size; //blank variable for the below if statment
            if (isPrompt) //if the user pressed the font change button, we want a new int for the font
            {
                var prompt = await InputTextDialogAsync(string.Format("Current size is {0}. Please enter a size.", dtFontSize));
                if (prompt == null || int.TryParse(prompt, out _) == false ) { return; }
                int.TryParse(prompt, out size);
                saveLoad.SaveHandler("dtFontSize", size);
            }
            else
            {
                size = dtFontSize; //if !isPrompt then that means the program requested the function, we pull the global font var
            }
            //the code below creates 2 new styles, then applies the new style to datagrid cells and headers
            var headerStyle = new Style(typeof(DataGridColumnHeader));
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(DataGridCell.FontSizeProperty, size - 2));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.FontSizeProperty, size));
            foreach (var col in WeekDT.Columns)
            {
                //applying the style
                col.HeaderStyle = headerStyle;
                col.CellStyle = cellStyle;
            }
        }

        //logic to change the notification text that would let the user know when the notifications will be sent
        /*
        private void NotifTextChanger()
        {
            if (notifOn && dTrigger != null)
            {
                var dtime = new DateTime(notificationTime.Ticks);
                NotifText.Text = string.Format("Notification will be sent daily at {0}.", dtime.ToString(@"h\:mm tt"));
            }
            else
            {
                NotifText.Text = "Notifications are turned off.";
            }
        }*/

        private void SetScheduleDays() 
        {
            //Following code identifies the following days of the week number and name. Starts with Monday
            DateTime dt = DateTime.Today.Next(DayOfWeek.Sunday);
            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                dt = DateTime.Today.Date;
            }
            DateTime[] days = {
                                dt.Next(DayOfWeek.Monday),
                                dt.Next(DayOfWeek.Tuesday),
                                dt.Next(DayOfWeek.Wednesday),
                                dt.Next(DayOfWeek.Thursday),
                                dt.Next(DayOfWeek.Friday),
                                dt.Next(DayOfWeek.Saturday),
                                dt.Next(DayOfWeek.Sunday), };

            TitleBlock.Text = string.Format("{0}, Week of {1} - {2}", dt.ToString("MMMM"), days[0].Day, days[6].Day);

            for (var i = 0; i < WeekDT.Columns.Count(); i++)
            {
                //column 0 should be employee name, then the week starts
                if (i == 0) 
                { 
                    WeekDT.Columns.ElementAt(i).Header = "Employee\nName";
                    continue;
                }
                currentDays.Add(days[i - 1]);
                WeekDT.Columns.ElementAt(i).Header = days[i - 1].ToString("dddd,\ndd");
            }
        }

        private void SetScheduleDays(DateTime targetDay)
        {
            //this overload does the same thing but for a targeted date on a calandar in the UI
            DateTime dt = targetDay.Previous(DayOfWeek.Sunday);
            if (targetDay.DayOfWeek == DayOfWeek.Sunday)
            {
                dt = targetDay.Date;
            }
            DateTime[] days = {
                                dt.Next(DayOfWeek.Monday),
                                dt.Next(DayOfWeek.Tuesday),
                                dt.Next(DayOfWeek.Wednesday),
                                dt.Next(DayOfWeek.Thursday),
                                dt.Next(DayOfWeek.Friday),
                                dt.Next(DayOfWeek.Saturday),
                                dt.Next(DayOfWeek.Sunday), };

            TitleBlock.Text = string.Format("{0}, Week of {1} - {2}", dt.ToString("MMMM"), days[0].Day, days[6].Day);

            for (var i = 0; i < WeekDT.Columns.Count(); i++)
            {
                //column 0 should be employee name, then the week starts
                if (i == 0) 
                {
                    WeekDT.Columns.ElementAt(i).Header = "Employee\nName";
                    continue;
                }
                WeekDT.Columns.ElementAt(i).Header = days[i - 1].ToString("dddd,\ndd");
            }
        }

        private void RefreshDT()
        {
            ///Refreshes the Datagrid element and all relevent data sources so that changes are visible
            ///Item sources must be nulled and then assigned to update properly
            ///Also issues an autosave command under "temp" file

            saveLoad.SaveHandler("temp", workers);
            WeekDT.ItemsSource = null;
            WeekDT.ItemsSource = workers;
            var targetThickness = 600 - (50 * workers.Count());
            if (targetThickness < 50) { targetThickness = 50; }
            WeekDT.Margin = new Thickness(0, 200, 0, targetThickness);
            EmployeeComboBox.ItemsSource = null;
            EmployeeComboBox.ItemsSource = workers;
            PresetComboBox.ItemsSource = null;
            PresetComboBox.ItemsSource = existingPresets;
            GroupSelectBox.ItemsSource = null;
            GroupSelectBox.ItemsSource = new ObservableCollection<string>(confirmedChats.Keys);
        }


        private async void WeekDT_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            ///Event for when the user edits the cell
            //find the cell that was called
            int pos_x = e.Column.DisplayIndex;
            int pos_y = e.Row.GetIndex();
            var selectedWorker = workers[pos_y];
            if (pos_x == 0)
            {
                //if position is 0, its the employee's name column, sends an edit name screen to user
                var input = await InputTextDialogAsync("Employee Name");
                if (input != null)
                {
                    selectedWorker.WName = input;
                }
                RefreshDT();
                return;
            }
            //else send time request to user
            TimeSpan[] setTimes = await InputTimeDialogAsync(currentDays[pos_x-1].ToString("dddd, dd"), pos_y, pos_x-1); 
            if (setTimes == null)
            {
                return; 
            } else if (setTimes[0].TotalMilliseconds < 0 && setTimes[1].TotalMilliseconds < 0) //if both times are < 0 then then make the time blank
            {
                selectedWorker.DayHours[pos_x - 1, 0] = new DateTime();
                selectedWorker.DayHours[pos_x - 1, 1] = new DateTime();
                //update visual aspect of the hours
                selectedWorker.SetCHours();
                RefreshDT();
                return;
            }
            //else combine the selected date with the timespan recieved from the input time
            DateTime[] colTimes = new DateTime[] {currentDays[pos_x - 1] + setTimes[0], currentDays[pos_x - 1] + setTimes[1]};

            selectedWorker.DayHours[pos_x - 1, 0] = colTimes[0];
            selectedWorker.DayHours[pos_x - 1, 1] = colTimes[1];
            
            selectedWorker.SetCHours();
            RefreshDT();
            return;

        }

        private async void InputConfirmChats()
        {
            Dictionary<string, int> scanResult = await tgb.ScanGroups();
            var sourceNames = new ObservableCollection<string>();
            var confirmedNames = new ObservableCollection<string>();
            var chatKeyset = confirmedChats.Keys;
            var sourceLb = new ListBox();
            var confirmedLb = new ListBox();
            var sourceTitle = new TextBlock();
            var confirmedTitle = new TextBlock();
            sourceTitle.Text = "Scanned Groups";
            confirmedTitle.Text = "Added Groups";
            sourceTitle.FontSize = 16;
            confirmedTitle.FontSize = 16;

            foreach(var name in scanResult.Keys)
            {
                if (chatKeyset.Contains(name))
                {
                    confirmedNames.Add(name);
                }
                else
                {
                    sourceNames.Add(name);
                }
            }

            sourceLb.ItemsSource = sourceNames;
            confirmedLb.ItemsSource = confirmedNames;

            var leftToRightAddB = new Button
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Content = "\xE72A"
            };
            var rightToLeftSubB = new Button
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Content = "\xE72B"
            };

            leftToRightAddB.Click += delegate (object sender, RoutedEventArgs e)
                { if (sourceLb.SelectedItem != null)
                    {
                        LeftToRightAddB_Click(sender, e, sourceLb.SelectedItem.ToString(), ref sourceNames, ref confirmedNames);
                    }
                };
            rightToLeftSubB.Click += delegate (object sender, RoutedEventArgs e)
                { if (confirmedLb.SelectedItem != null)
                    {
                        RightToLeftSubB_Click(sender, e, confirmedLb.SelectedItem.ToString(), ref sourceNames, ref confirmedNames);
                    }
                };

            var buttonGrid = new UniformGrid
            {
                Columns = 1
            };

            buttonGrid.Children.Add(leftToRightAddB);
            buttonGrid.Children.Add(rightToLeftSubB);
            buttonGrid.HorizontalAlignment = HorizontalAlignment.Center;
            var leftSide = new UniformGrid
            {
                Columns = 1,
                RowSpacing = 5
            };
            var rightSide = new UniformGrid
            {
                Columns = 1,
                RowSpacing = 10
            };
            leftSide.Children.Add(sourceTitle);
            leftSide.Children.Add(sourceLb);
            rightSide.Children.Add(confirmedTitle);
            rightSide.Children.Add(confirmedLb);
            UniformGrid.SetRowSpan(confirmedLb, 2);
            UniformGrid.SetRowSpan(sourceLb, 2);
            var sCB = new SolidColorBrush(Colors.White);
            sourceLb.Background = sCB;
            confirmedLb.Background = sCB;
            rightToLeftSubB.Background = sCB;
            leftToRightAddB.Background = sCB;
            var containedGrid = new UniformGrid
            {
                ColumnSpacing = 15,
                Rows = 1,
                MinHeight = 200
            };
            containedGrid.Children.Add(leftSide);
            containedGrid.Children.Add(buttonGrid);
            containedGrid.Children.Add(rightSide);
            var dialog = new ContentDialog
            {
                Content = containedGrid,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Title = "Available Groups"
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                foreach(var name in sourceNames)
                {
                    if (chatKeyset.Contains(name))
                    {
                        confirmedChats.Remove(name);
                    }
                }
                foreach(var name in confirmedNames)
                {
                    if (!chatKeyset.Contains(name))
                    {
                        confirmedChats[name] = scanResult[name];
                    }
                }
                saveLoad.SaveHandler("confirmedChats", confirmedChats);
            }
            RefreshDT();
            return;
        }

        private void RightToLeftSubB_Click(object sender, RoutedEventArgs e, string name, ref ObservableCollection<string> source, ref ObservableCollection<string> confirmed)
        {
            if (name != null)
            {
                confirmed.Remove(name);
                source.Add(name);
            }
        }

        private void LeftToRightAddB_Click(object sender, RoutedEventArgs e, string name, ref ObservableCollection<string> source, ref ObservableCollection<string> confirmed)
        {
            if (name != null)
            {
                source.Remove(name);
                confirmed.Add(name);
            }
        }

        private async Task<TimeSpan[]> InputTimeDialogAsync(string title,int workerPos, int dayPos)
        {
            var input = new TimePicker();
            var input2 = new TimePicker();
            var titleHours = new TextBlock();
            var additionalDayTitle = new TextBlock();
            var leaveBlankText = new TextBlock();
            var dayChecks = new UniformGrid();
            var stkP = new StackPanel();
            input.MinuteIncrement = 15;
            input2.MinuteIncrement = 15;
            titleHours.Text = "Select starting and ending hours.";
            additionalDayTitle.Text = "Copy this time to additional days?";
            leaveBlankText.Text = "Leave blank to clear the time.";
            leaveBlankText.FontSize = 12;
            leaveBlankText.Foreground = new SolidColorBrush(Colors.Gray);
            var buttonChecks = new ObservableCollection<ToggleButton>();
            for (var i = 0; i<7; i++)
            {
                var tempB = new ToggleButton
                {
                    Width = 100,
                    Content = currentDays[i].ToString("dddd")
                };
                if (i == dayPos) { tempB.IsEnabled = false; }
                buttonChecks.Add(tempB);
            }
            dayChecks.Orientation = Orientation.Horizontal;
            dayChecks.Rows = 2;
            foreach (var chck in buttonChecks)
            {
                dayChecks.Children.Add(chck);
            }

            stkP.Spacing = 5;
            stkP.Children.Add(titleHours);
            stkP.Children.Add(leaveBlankText);
            stkP.Children.Add(input);
            stkP.Children.Add(input2);
            stkP.Children.Add(additionalDayTitle);
            stkP.Children.Add(dayChecks);
            var dialog = new ContentDialog
            {
                Content = stkP,
                VerticalContentAlignment = VerticalAlignment.Center,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                TimeSpan[] rTimes = new TimeSpan[] { input.Time, input2.Time};
                if (input.Time.TotalMilliseconds < 0 && input2.Time.TotalMilliseconds < 0)
                {
                    for (var i=0; i<buttonChecks.Count; i++)
                    {
                        if (buttonChecks[i].IsChecked == true && buttonChecks[i].IsEnabled == true)
                        {
                            workers[workerPos].DayHours[i, 0] = new DateTime();
                            workers[workerPos].DayHours[i, 1] = new DateTime();
                        }
                    }
                    return rTimes;
                }
                else if (input.Time.TotalMilliseconds < 0 || input2.Time.TotalMilliseconds < 0) 
                {
                    var emptyTimeCatch = await InputBoolDialogAsync("Error: A time slot was left blank.\n\nWould you like to input a time again?");
                    if (emptyTimeCatch == true)
                    {
                        rTimes = await InputTimeDialogAsync(title, workerPos, dayPos);
                    }
                    else
                    {
                        return null;
                    }
                }
                for (var i=0; i<buttonChecks.Count; i++)
                {
                    if (buttonChecks[i].IsChecked == true && buttonChecks[i].IsEnabled == true)
                    {
                        workers[workerPos].DayHours[i, 0] = currentDays[i] + rTimes[0];
                        workers[workerPos].DayHours[i, 1] = currentDays[i] + rTimes[1];
                    }
                }
                return rTimes;
            }else
                return null;
        }
        //This is the logic of the notification menu, for now it'll be hidden
        /*private async Task<bool> InputNotifTimeAsync()
        {
            var grd = new UniformGrid();
            var timeSelectTxt = new TextBlock();
            var timeSelect = new TimePicker();
            var subGrid = new UniformGrid();
            var wantNotifText = new TextBlock();
            var wantNotifBool = new CheckBox();
            var dialog = new ContentDialog();
            dialog.Title = "Notification Settings";
            timeSelect.MinuteIncrement = 15;
            timeSelectTxt.Text = "Select time to send out notifications";
            grd.Columns = 1;
            grd.RowSpacing = 10;
            grd.Children.Add(timeSelectTxt);
            grd.Children.Add(timeSelect);
            subGrid.Rows = 1;
            wantNotifText.Text = "Enable or disable notifications";
            wantNotifText.VerticalAlignment = VerticalAlignment.Center;
            wantNotifBool.IsChecked = notifOn;
            subGrid.ColumnSpacing = 20;
            subGrid.Children.Add(wantNotifText);
            subGrid.Children.Add(wantNotifBool);
            grd.Children.Add(subGrid);
            dialog.Content = grd;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.SecondaryButtonText = "Cancel";
            dialog.PrimaryButtonText = "Save";
            dialog.DefaultButton = ContentDialogButton.Primary;

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (timeSelect.SelectedTime == null && wantNotifBool.IsChecked == true)
                {
                    if (await InputBoolDialogAsync("Error: Time was left blank.\n\nWould you like to input a time again?"))
                    {
                        return await InputNotifTimeAsync();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    notifOn = (bool) wantNotifBool.IsChecked;
                    saveLoad.SaveHandler("notifBool", notifOn);
                    if (!notifOn) 
                    { 
                        if (dTrigger != null)
                        {
                            dTrigger.CancelTask();
                        }

                        return false;
                    }
                    notificationTime = (TimeSpan) timeSelect.SelectedTime;
                    saveLoad.SaveHandler("notifTime", notificationTime);

                    return true;
                }

            }
            return false;
        }*/

        private async Task<string> InputTextDialogAsync(string title)
        {
            var input = new TextBox
            {
                AcceptsReturn = false,
                Height = 32
            };
            var dialog = new ContentDialog
            {
                Content = input,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return input.Text;
            else
                return null;

        }

        private async Task<bool> InputBoolDialogAsync(string title)
        {
            var input = new TextBlock
            {
                Text = title,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20
            };
            var dialog = new ContentDialog
            {
                Content = input,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return true;
            else
                return false;
        }
        
        private async void AddEmployeeB_Click(object sender, RoutedEventArgs e)
        {
            string employeeName = await InputTextDialogAsync("Employee Name");
            if (employeeName == "" || employeeName == null)
            {
                return;
            }
            workers.Add(new Worker(employeeName));
            RefreshDT();
            
        }

        private async void SendToGrpB_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(RenderGrid);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var displayInfo = DisplayInformation.GetForCurrentView();
            var fileMade = await ApplicationData.Current.LocalFolder.CreateFileAsync("TestSendToGrp" + ".png", CreationCollisionOption.ReplaceExisting);
            using (var stream = await fileMade.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                    BitmapAlphaMode.Premultiplied,
                                    (uint)rtb.PixelWidth,
                                    (uint)rtb.PixelHeight,
                                    300,
                                    300,
                                    pixels);
                await encoder.FlushAsync();
            }
            FileStream fs = File.Open(fileMade.Path, FileMode.Open);
            if (GroupSelectBox.SelectedItem != null)
            {
                var targetGroup = GroupSelectBox.SelectedItem.ToString();
                tgb.SendPhoto(fs, "Schedule for " + TitleBlock.Text, confirmedChats[targetGroup]);
            }
        }

        private void WeekSelect_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            SetScheduleDays(WeekSelect.Date.Value.DateTime);
        }

        private async void ClearB_Click(object sender, RoutedEventArgs e)
        {
            if (await InputBoolDialogAsync("Are you sure you want to clear the chart?") == true)
            {
                foreach (Worker w in workers)
                {
                    w.ResetHours();
                }
                RefreshDT();

            }
        }

        private void ChangeFont_Click(object sender, RoutedEventArgs e)
        {
            FontChangeHandler(true);
        }

        private async void SavePreB_Click(object sender, RoutedEventArgs e)
        {
            var selected = PresetComboBox.SelectedItem;
            string presetName;
            if (selected == null)
            {
                presetName = await InputTextDialogAsync("Preset Name");
                if (presetName == null)
                {
                    return;
                }
                existingPresets.Add(presetName);
                RefreshDT();
            }
            else
            {
                presetName = selected.ToString();
                if (!await InputBoolDialogAsync(String.Format("Overwrite preset {0}?", presetName)))
                {
                    return;
                }
            }
            saveLoad.SaveHandler("presetList", existingPresets);
            saveLoad.SaveHandler(presetName, workers);

        }

        private async void RemovePreB_Click(object sender, RoutedEventArgs e)
        {
            var targetPreset = PresetComboBox.SelectedItem;
            if (targetPreset == null) { return; }
            if (await InputBoolDialogAsync(string.Format("Delete Preset: {0}?", targetPreset.ToString())) == true)
            {
                existingPresets.Remove(targetPreset.ToString());
                saveLoad.SaveHandler("presetList", existingPresets);
                RefreshDT();
            }
        }

        private async void EditPreB_Click(object sender, RoutedEventArgs e)
        {
            var targetPreset = PresetComboBox.SelectedItem;
            if (targetPreset == null) { return; }
            var newName = await InputTextDialogAsync("Edit Preset Name");
            if ( newName != null)
            {
                existingPresets.Remove(targetPreset.ToString());
                existingPresets.Add(newName);
                saveLoad.SaveHandler("presetList", existingPresets);
                RefreshDT();
            }
        }


        private void LoadPreB_Click(object sender, RoutedEventArgs e)
        {
            if (PresetComboBox.SelectedItem == null) { return; }
            workers = saveLoad.LoadHandler(PresetComboBox.SelectedItem.ToString(), 'w');
            RefreshDT();
        }

        private async void EditEmployB_Click(object sender, RoutedEventArgs e)
        {
            var targetEmployee = EmployeeComboBox.SelectedItem;
            if (targetEmployee == null) { return; }
            var newName = await InputTextDialogAsync("Edit Employee Name");
            if (newName != null)
            {
                workers[EmployeeComboBox.SelectedIndex].WName = newName;
                RefreshDT();
            }

        }

        private void RemoveEmployB_Click(object sender, RoutedEventArgs e)
        {
            Worker target = (Worker)EmployeeComboBox.SelectedItem;
            if (target != null)
            {
                workers.Remove(target);
                RefreshDT();
            }
        }

        //Notif Menu hidden for now, will be enabled in a future build
        /*private async void NotifMenu_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = await InputNotifTimeAsync();
            if (isValid)
            {
                if (dTrigger != null)
                {
                    dTrigger.CancelTask();
                }
                NotifHandler();
            }
            NotifTextChanger();
        }*/

        private void GroupMenu_Click(object sender, RoutedEventArgs e)
        {
            InputConfirmChats();
        }

        private void GroupSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupSelectBox.SelectedItem != null)
            {
                LocationBlock.Text = GroupSelectBox.SelectedItem.ToString();
                SendToGrpB.IsEnabled = true;
            }
        }
    }

}
