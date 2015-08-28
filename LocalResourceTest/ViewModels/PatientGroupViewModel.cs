using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    public class PatientGroupViewModel : INotifyPropertyChanged
    {
        public PatientGroupViewModel(string patientId)
        {
            PatientId = patientId;
        }

        public string PatientId
        {
            get;
            set;
        }

        public int InstanceCount
        {
            get { return _instanceCount; }
            set
            {
                if (value == _instanceCount) return;
                _instanceCount = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("InstanceCount"));
            }
        }
        int _instanceCount = 1;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
