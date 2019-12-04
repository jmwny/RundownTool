using System;
using System.Collections.Generic;
using System.Text;

namespace RundownTool.Models
{
    class RundownItem : Notifier
    {
        // From Template
        private string _hospitalFullName;
        public string HospitalFullName
        {
            get { return _hospitalFullName;  }
            set
            {
                if (value != this._hospitalFullName)
                {
                    _hospitalFullName = value;
                    OnPropertyChanged("HospitalFullName");
                }
            }
        }

        private string _tour;
        public string Tour
        {
            get { return _tour; }
            set
            {
                if (value != this._tour)
                {
                    _tour = value;
                    OnPropertyChanged("Tour");
                }
            }
        }

        private string _unitName;
        public string UnitName
        {
            get { return _unitName; }
            set
            {
                if (value != this._unitName)
                {
                    _unitName = value;
                    OnPropertyChanged("UnitName");
                }
            }
        }

        private string _startTime;
        public string StartTime
        {
            get { return _startTime; }
            set
            {
                if (value != this._startTime)
                {
                    _startTime = value;
                    OnPropertyChanged("StartTime");
                }
            }
        }

        private string _dateSymbolic;
        public string DateSymbolic
        {
            get { return _dateSymbolic; }
            set
            {
                if (value != this._dateSymbolic)
                {
                    _dateSymbolic = value;
                    OnPropertyChanged("DateSymbolic");
                }
            }
        }

        //From Vehicles
        private string _vehicleNumber;
        public string VehicleNumber
        {
            get { return _vehicleNumber; }
            set
            {
                if (value != this._vehicleNumber)
                {
                    _vehicleNumber = value;
                    OnPropertyChanged("VehicleNumber");
                }
            }
        }

        private string _radio1;
        public string Radio1
        {
            get { return _radio1; }
            set
            {
                if (value != this._radio1)
                {
                    _radio1 = value;
                    OnPropertyChanged("Radio1");
                }
            }
        }

        private string _radio2;
        public string Radio2
        {
            get { return _radio2; }
            set
            {
                if (value != this._radio2)
                {
                    _radio2 = value;
                    OnPropertyChanged("Radio2");
                }
            }
        }

        // From export
        private string _crewName1;
        public string CrewName1
        {
            get { return _crewName1; }
            set
            {
                if (value != this._crewName1)
                {
                    _crewName1 = value;
                    OnPropertyChanged("CrewName1");
                }
            }
        }

        private string _crewShield1;
        public string CrewShield1
        {
            get { return _crewShield1; }
            set
            {
                if (value != this._crewShield1)
                {
                    _crewShield1 = value;
                    OnPropertyChanged("CrewShield1");
                }
            }
        }

        private string _crewName2;
        public string CrewName2
        {
            get { return _crewName2; }
            set
            {
                if (value != this._crewName2)
                {
                    _crewName2 = value;
                    OnPropertyChanged("CrewName2");
                }
            }
        }

        private string _crewShield2;
        public string CrewShield2
        {
            get { return _crewShield2; }
            set
            {
                if (value != this._crewShield2)
                {
                    _crewShield2 = value;
                    OnPropertyChanged("CrewShield2");
                }
            }
        }
    }
}
