using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace TestWpfApp
{
    public class TestData : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private int _age;
        private string _lastName;
        private string _name;
        private string _type;
        private IEnumerable<string> _types;

        public TestData()
        {
            Age = -1;
            Name = string.Empty;
            LastName = string.Empty;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.Equals(_name, value, StringComparison.Ordinal)) return;

                _name = value;

                if (string.IsNullOrWhiteSpace(_name))
                    _errors[nameof(Name)] = new List<string> {"Last Name must be set"};
                
            else
            _errors.Remove(nameof(Name));

                OnPropertyChanged();
                OnErrorsChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if(string.Equals(_lastName, value, StringComparison.Ordinal)) return;

                _lastName = value;

                if (string.IsNullOrWhiteSpace(_lastName))
                    _errors[nameof(LastName)] = new List<string> {"Last Name must be set"};
                else
                    _errors.Remove(nameof(LastName));

                OnPropertyChanged();
                OnErrorsChanged();
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if(_age.Equals(value)) return;

                _age = value;
                List<string> error = new List<string>();

                if (_age > 100)
                    error.Add("To Old");
                if (_age < 19)
                    error.Add("To young");
                if (_age < 3)
                    error.Add("Way To young");
                if (error.Count == 0)
                    _errors.Remove(nameof(Age));
                else
                    _errors[nameof(Age)] = error;

                OnPropertyChanged();
                OnErrorsChanged();
            }
        }
        
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null) return null;
            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }

        public string[] Names { get; set; }

        [Bindable(false)]
        public bool HasErrors => _errors.Count != 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged([CallerMemberName]string name = null) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(name));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public override string ToString()
        {
            return Name + " " + LastName;
        }
    }
}