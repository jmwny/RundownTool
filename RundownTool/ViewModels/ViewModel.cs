using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using RundownTool.Models;

namespace RundownTool.ViewModels
{
    class ViewModel : Notifier
    {
        static readonly string resourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
        static readonly string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
        static readonly string templateFile = Path.Combine(resourceDirectory, "template.csv");
        static readonly string vehiclesFile = Path.Combine(resourceDirectory, "vehicles.csv");

        #region Properties
        private RundownItem _gridSelectedItem;
        public RundownItem GridSelectedItem
        {
            get { return _gridSelectedItem; }
            set
            {
                if (value != this._gridSelectedItem)
                {
                    _gridSelectedItem = value;
                    OnPropertyChanged("GridSelectedItem");
                }
            }
        }

        private int _gridSelectedIndex;
        public int GridSelectedIndex
        {
            get { return _gridSelectedIndex; }
            set
            {
                if (value != this._gridSelectedIndex)
                {
                    _gridSelectedIndex = value;
                    OnPropertyChanged("GridSelectedIndex");
                }
            }
        }

        private bool _exportButtonEnable;
        public bool ExportButtonEnable
        {
            get { return _exportButtonEnable; }
            set
            {
                if (value != this._exportButtonEnable)
                {
                    _exportButtonEnable = value;
                    OnPropertyChanged("ExportButtonEnable");
                }
            }
        }

        private bool _processButtonEnable;
        public bool ProcessButtonEnable
        {
            get { return _processButtonEnable; }
            set
            {
                if (value != this._processButtonEnable)
                {
                    _processButtonEnable = value;
                    OnPropertyChanged("ProcessButtonEnable");
                }
            }
        }

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (value != this._statusText)
                {
                    _statusText = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }

        private ObservableCollection<RundownItem> _rundownItems;
        public ObservableCollection<RundownItem> RundownItems
        {
            get { return _rundownItems; }
            set
            {
                if (value != this._rundownItems)
                {
                    _rundownItems = value;
                    OnPropertyChanged("RundownItems");
                }
            }
        }
        #endregion

        public ViewModel()
        {
            RundownItems = new ObservableCollection<RundownItem>();

            try {
                LoadTemplate();
                StatusText = "Template successfully loaded";
                ExportButtonEnable = true;
                ProcessButtonEnable = false;
            }
            catch {
                StatusText = "Failed to load templates - Resources available?";
                ExportButtonEnable = ProcessButtonEnable = false;
            }
        }

        internal void LoadTemplate()
        {
            // read template
            using (var reader = new StreamReader(templateFile))
            using (var csv = new CsvReader(reader))
                while (csv.Read())
                {
                    RundownItem item = new RundownItem
                    {
                        HospitalFullName = csv.GetField(0).Trim(),
                        Tour = csv.GetField(1).Trim(),
                        UnitName = csv.GetField(2).Trim(),
                        StartTime = csv.GetField(3).Trim(),
                        DateSymbolic = csv.GetField(4).Trim()
                    };
                    RundownItems.Add(item);
                }
            // read vehicles and fill in the rest of the RundownItem
            // also to be called when we need to reload vehicles after an update
            using (var reader = new StreamReader(vehiclesFile))
            using (var csv = new CsvReader(reader))
                while (csv.Read())
                {
                    foreach (RundownItem item in RundownItems)
                        if (item.UnitName == csv.GetField(0).Trim())
                        {
                            item.VehicleNumber = csv.GetField(1).Trim();
                            item.Radio1 = csv.GetField(2).Trim();
                            item.Radio2 = csv.GetField(3).Trim();
                        }
                }
        }

        internal void MergeExport(string exportPath, DateTime selectedDate)
        {
            /* TODO: Document the export.csv fields
             */
            string dateCurrent = selectedDate.ToShortDateString();
            string dateNext = selectedDate.AddDays(1).ToShortDateString();
            string dateActual;

            // we only care about 911 tours
            using (var reader = new StreamReader(exportPath))
            using (var csv = new CsvReader(reader))
            {
                // Validate that we have 14 records
                // If emp last name wasn't selected on CSV export, 13 records are expected
                csv.Read();
                csv.ReadHeader();
                if (csv.Context.HeaderRecord.Length != 14)
                {
                    StatusText = "Failure - Invalid Export File, header count is low";
                    return;
                }
                // Parse the rest of the file
                while (csv.Read())
                    if (csv.GetField(5).StartsWith("911-"))
                    {
                        string unit = csv.GetField(6).Split(' ')[0];
                        string shield = Regex.Match(csv.GetField(13), @"\b\d{4}\b").Value;
                        // Last name
                        int lastNameIndex = csv.GetField(13).IndexOf(shield);
                        string lastName = csv.GetField(13).Substring(0, lastNameIndex).Trim();

                        foreach (RundownItem item in RundownItems)
                            if (item.UnitName == unit)
                            {
                                dateActual = (item.DateSymbolic == "CURRENT") ? dateCurrent : dateNext;
                                if (dateActual == csv.GetField(7))
                                    if (item.StartTime == csv.GetField(8))
                                    {
                                        if (item.CrewName1 == null)
                                        {
                                            item.CrewName1 = lastName;
                                            item.CrewShield1 = shield;
                                        }
                                        else
                                        {
                                            item.CrewName2 = lastName;
                                            item.CrewShield2 = shield;
                                        }
                                    }
                            }
                    }
            }
            StatusText = "Export processed";
            ProcessButtonEnable = true;
        }

        internal async void ProcessExport()
        {
            List<string> hospitalList = new List<string>();

            // Generate list of unique hospitals
            foreach (RundownItem item in RundownItems)
            {
                if (!hospitalList.Contains(item.HospitalFullName))
                    hospitalList.Add(item.HospitalFullName);
            }

            // disable buttons
            ExportButtonEnable = false;
            ProcessButtonEnable = false;

            // Read and repl
            foreach (string hospital in hospitalList)
            {
                // read
                string fileContents = string.Empty;
                string filePath = Path.Combine(resourceDirectory, Path.ChangeExtension(hospital, ".rtf"));
                if (!File.Exists(filePath))
                {
                    StatusText = $"Failed to load resource - RTF Template for '{hospital}' not found.";
                    continue;
                }

                StatusText = $"Generating Rundown for '{hospital}'";
                await Task.Run(() =>
                {
                    fileContents = File.ReadAllText(filePath);
                    // repl (vehicle, name[1,2], shield[1,2], radio[1,2])
                    fileContents = fileContents.Replace("!", DateTime.Now.AddDays(1).ToShortDateString());
                    foreach (RundownItem item in RundownItems)
                    {
                        if (item.HospitalFullName == hospital)
                        {
                            fileContents = InsertValue(fileContents, item.VehicleNumber);
                            fileContents = InsertValue(fileContents, item.CrewName1);
                            fileContents = InsertValue(fileContents, item.CrewShield1);
                            fileContents = InsertValue(fileContents, item.Radio1);
                            fileContents = InsertValue(fileContents, item.CrewName2);
                            fileContents = InsertValue(fileContents, item.CrewShield2);
                            fileContents = InsertValue(fileContents, item.Radio2);
                        }
                    }

                    // save
                    if (!Directory.Exists(outputDirectory))
                        Directory.CreateDirectory(outputDirectory);
                    File.WriteAllText(Path.Combine(outputDirectory, Path.ChangeExtension(hospital, ".rtf")), fileContents);
                });
            }
            System.Diagnostics.Process.Start("explorer.exe", outputDirectory);
            // enable buttons
            ExportButtonEnable = true;
            ProcessButtonEnable = true;
            StatusText = "Completed";
        }

        private string InsertValue(string s, string v)
        {
            if (v == null)
                v = string.Empty;
            int loc = s.IndexOf("@");
            string res = s.Remove(loc, 1).Insert(loc, v);
            return res;
        }

        internal void CellEditEnding(string colHeader, string newValue)
        {
            StatusText = $"Replacing '{colHeader}' value with '{newValue}' on unit '{GridSelectedItem.UnitName}'";

            foreach (RundownItem item in RundownItems)
                if (item.UnitName == GridSelectedItem.UnitName)
                {
                    if (colHeader == "Vehicle")
                        item.VehicleNumber = newValue.Trim();
                    else if (colHeader == "Radio #1")
                        item.Radio1 = newValue.Trim();
                    else if (colHeader == "Radio #2")
                        item.Radio2 = newValue.Trim();
                }
        }

        internal void SaveVehicles()
        {
            List<string> vehiclesList = new List<string>();
            foreach (RundownItem item in RundownItems)
            {
                string vehicleInfo = $"{item.UnitName},{item.VehicleNumber},{item.Radio1},{item.Radio2}";
                if (!vehiclesList.Contains(vehicleInfo))
                    vehiclesList.Add(vehicleInfo);
            }
            if (File.Exists(vehiclesFile))
                File.WriteAllLines(vehiclesFile, vehiclesList);
        }
    }
}
