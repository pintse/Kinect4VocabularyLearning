using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;

namespace Kinect.Toolbox
{
    public class MagneticPropertyHolder2
    {
        private static ILog log = LogManager.GetLogger(typeof(MagneticPropertyHolder2));
        public static readonly DependencyProperty IsMagneticProperty2 = DependencyProperty.RegisterAttached("IsMagnetic2", typeof(bool), typeof(MagneticPropertyHolder2),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsMagneticChanged));

        static void OnIsMagneticChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //log.Debug("e.Property.Name::" + e.Property.Name);
            FrameworkElement element = d as FrameworkElement;
            if ((bool)e.NewValue)
            {
                if (!MouseController2.Current.MagneticsControl.Contains(element))
                    MouseController2.Current.MagneticsControl.Add(element);
            }
            else
            {
                if (MouseController2.Current.MagneticsControl.Contains(element))
                    MouseController2.Current.MagneticsControl.Remove(element);
            }
        }

        public static void SetIsMagnetic(UIElement element, Boolean value)
        {
            element.SetValue(IsMagneticProperty2, value);
        }

        public static bool GetIsMagnetic(UIElement element)
        {
            return (bool)element.GetValue(IsMagneticProperty2);
        }
    }
}