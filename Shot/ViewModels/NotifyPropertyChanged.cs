using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shot.ViewModels
{

    /// <summary>
    /// Implementation of INotifyPropertyChanged providing various conveniences
    /// around getting, setting, and raising properties.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        #region Fields
        private readonly Dictionary<string, object> _propertyBackingDictionary = new Dictionary<string, object>();
        #endregion Fields

        #region Properties
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Properties

        #region Methods
        /// <summary>
        /// Clears the property changed handlers.
        /// </summary>
        protected void ClearPropertyChangedHandlers()
        {
            PropertyChanged = null;
        }

        /// <summary>
        /// Raises a PropertyChanged event for all properties that have set a
        /// value using the SetPropertyValue function.
        /// </summary>
        internal void RaiseAllPropertiesChanged()
        {
            foreach (var prop in _propertyBackingDictionary)
                OnPropertyChanged(prop.Key);
        }

        /// <summary>
        /// Set the specified field, value and propertyName.   Use this set with a backing field and it automatically raises the On Property change
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="field">Field.</param>
        /// <param name="value">Value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Gets the property value.  Use this when No backing field is required, but you want to track property changes
        /// </summary>
        /// <returns>The property value.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected T GetPropertyValue<T>(T defaultValue = default(T), [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            if (_propertyBackingDictionary.TryGetValue(propertyName, out object value))
                return (T)value;

            if (!EqualityComparer<T>.Default.Equals(defaultValue, default(T)))
                _propertyBackingDictionary[propertyName] = defaultValue;

            return defaultValue;
        }

        protected T GetPropertyValue<T>(Func<T> defaultFactory, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            if (_propertyBackingDictionary.TryGetValue(propertyName, out object value))
                return (T)value;

            var defaultFactoryValue = defaultFactory();
            if (!EqualityComparer<T>.Default.Equals(defaultFactoryValue, default(T)))
                _propertyBackingDictionary[propertyName] = defaultFactoryValue;

            return defaultFactoryValue;
        }


        /// <summary>
        /// Sets the property value.  Use this when No backing field is required, but you want to track property changes
        /// </summary>
        /// <returns><c>true</c>, if property value was set, <c>false</c> otherwise.</returns>
        /// <param name="newValue">New value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool SetPropertyValue<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            if (EqualityComparer<T>.Default.Equals(newValue, GetPropertyValue<T>(default(T), propertyName))) return false;

            _propertyBackingDictionary[propertyName] = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Ons the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Methods
    }

}
