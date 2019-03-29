using System;
using Rbx2Source.Reflection;

namespace Rbx2Source.Animating
{
    static class EasingUtil
    {
        // Complex Easing Styles

        private static float bounceBase = 7.5625f;
        private static double tau = Math.PI * 2;

        private static float bounce(float t)
        {
            if (t < .36363636)
            {
                return bounceBase * t * t;
            }
            else if (t < .72727272)
            {
                t -= .54545454f;
                return bounceBase * t * t + .75f;
            }
            else if (t < .90909090)
            {
                t -= .81818181f;
                return bounceBase * t * t + .9375f;
            }
            else
            {
                t -= .95454545f;
                return bounceBase * t * t + .984375f;
            }
        }

        private static float cubic(float t)
        {
            return t * t * t;
        }

        // Easing Directions.

        private static float easeIn(float t, Func<float,float> func)
        {
            return func(t);
        }

        private static float easeOut(float t, Func<float, float> func)
        {
            return 1 - func(1 - t);
        }

        private static float easeInOut(float t, Func<float,float> func)
        {
            float result;
            t *= 2f;

            if (t < 1)
                result = easeIn(t, func) * 0.5f;
            else
                result = 0.5f + (easeOut(t - 1, func) * 0.5f);

            return result;
        }

        public static float GetEasing(EasingStyle style, EasingDirection direction, float percent)
        {
            if (style == EasingStyle.Bounce)
            {
                float result;

                if (direction == EasingDirection.Out)
                    result = 1 - easeOut(percent, bounce);
                else if (direction == EasingDirection.In)
                    result = 1 - bounce(percent);
                else
                    result = 1 - easeInOut(percent, bounce);

                return result;
            }
            else if (style == EasingStyle.Elastic)
            {
                double result;

                if (direction == EasingDirection.InOut)
                {
                    double t = ((double)percent * 2) - 1;
                    double inOut = Math.Pow(2, 10 * t) * Math.Sin((t - 0.1125) * tau / 0.45);

                    if (t < 0)
                        inOut = 1 - (-.5 * inOut);
                    else
                        inOut = 1 - (1 + .5 * inOut);

                    result = inOut;
                }
                else
                {
                    double t = percent;
                    double value = (1 + Math.Pow(2, -10 * t) * Math.Sin((t - 0.925) * tau / 0.3));

                    if (direction == EasingDirection.In)
                        value = 1 - value;

                    result = value;
                }

                return (float)result;
            }
            else if (style == EasingStyle.Cubic)
            {
                float result;

                if (direction == EasingDirection.Out)
                    result = 1 - easeOut(percent, cubic);
                else if (direction == EasingDirection.In)
                    result = 1 - cubic(percent);
                else
                    result = 1 - easeInOut(percent, cubic);

                return result;
            }
            else if (style == EasingStyle.Linear)
            {
                return 1 - percent;
            }
            else // Constant
            {
                float result;

                if (direction == EasingDirection.Out)
                    result = 1;
                else if (direction == EasingDirection.In)
                    result = 0;
                else
                    result = 0.5f;

                return result;
            }
        }
    }
}