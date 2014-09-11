using System;
using System.ComponentModel;
using System.Linq.Expressions;
using log4net;

namespace Kinect.Toolbox
{
    public abstract class Notifier : INotifyPropertyChanged
    {
        private static ILog log = LogManager.GetLogger(typeof(Notifier));
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) 
                return;
            
            string propertyName = memberExpression.Member.Name;
            if (PropertyChanged != null)
            {
                //log.Debug("RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)");
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
