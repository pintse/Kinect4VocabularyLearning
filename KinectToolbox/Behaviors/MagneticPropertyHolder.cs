using System;
using System.Windows;
using log4net;

namespace Kinect.Toolbox
{
    public class MagneticPropertyHolder
    {
        private static ILog log = LogManager.GetLogger(typeof(MagneticPropertyHolder));
        public static readonly DependencyProperty IsMagneticProperty = DependencyProperty.RegisterAttached("IsMagnetic", typeof(bool), typeof(MagneticPropertyHolder),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsMagneticChanged));

        static void OnIsMagneticChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //log.Debug("e.Property.Name::" + e.Property.Name);
            FrameworkElement element = d as FrameworkElement;
            if ((bool)e.NewValue)
            {
                if (!MouseController.Current.MagneticsControl.Contains(element))
                    MouseController.Current.MagneticsControl.Add(element);
            }
            else
            {
                if (MouseController.Current.MagneticsControl.Contains(element))
                    MouseController.Current.MagneticsControl.Remove(element);
            }
        }

        public static void SetIsMagnetic(UIElement element, Boolean value)
        {
            element.SetValue(IsMagneticProperty, value);
        }

        public static bool GetIsMagnetic(UIElement element)
        {
            return (bool)element.GetValue(IsMagneticProperty);
        }
    }
}
