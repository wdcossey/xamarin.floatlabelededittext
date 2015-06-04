using Android.Annotation;
using Android.OS;
using Android.Views;
using Java.Util.Concurrent.Atomic;

namespace Xamarin.FloatLabeledEditText
{
  public class Utils
  {
    private static readonly AtomicInteger NextGeneratedId = new AtomicInteger(1);

    /// <summary>
    /// <para>Generate a value suitable for use in {@link #setId(int)}.<br/>
    /// This value will not collide with ID values generated at build time by aapt for R.id.</para>
    /// </summary>
    /// <returns>a generated ID value</returns>
    private static int generateViewId()
    {
      for (;;)
      {
        var result = NextGeneratedId.Get();

        // aapt-generated IDs have the high byte nonzero; clamp to the range under that.
        var newValue = result + 1;

        if (newValue > 0x00FFFFFF)
          newValue = 1; // Roll over to 1, not 0.

        if (NextGeneratedId.CompareAndSet(result, newValue))
        {
          return result;
        }
      }
    }

    [SuppressLint(Value = new[] { "NewApi" })]
    public static int generateId()
    {
      return Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1 ? generateViewId() : View.GenerateViewId();
    }
  }
}