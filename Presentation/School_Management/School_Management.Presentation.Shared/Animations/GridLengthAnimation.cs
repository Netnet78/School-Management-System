using System.Windows;
using System.Windows.Media.Animation;

namespace School_Management.Presentation.Shared.Animations
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(nameof(From), typeof(GridLength?), typeof(GridLengthAnimation));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(nameof(To), typeof(GridLength?), typeof(GridLengthAnimation));

        public override Type TargetPropertyType => typeof(GridLength);

        public GridLength? From
        {
            get => (GridLength?)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public GridLength? To
        {
            get => (GridLength?)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromValue = From?.Value ?? ((GridLength)defaultOriginValue).Value;
            double toValue = To?.Value ?? ((GridLength)defaultDestinationValue).Value;
            GridUnitType fromValueUnitType = ((GridLength)defaultOriginValue).GridUnitType;
            GridUnitType toValueUnitType = ((GridLength)defaultDestinationValue).GridUnitType;

            if (animationClock.CurrentProgress == null)
            {
                return new GridLength(fromValue, fromValueUnitType);
            }

            double progress = animationClock.CurrentProgress.Value;
            double value = ((toValue - fromValue) * progress) + fromValue;

            return new GridLength(value, toValueUnitType);
        }
    }
}
