using System;
using Android.Animation;
using Android.Annotation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Exception = System.Exception;

namespace Xamarin.FloatLabeledEditText
{
  public class FloatLabeledEditText : FrameLayout
  {
    private const int DEFAULT_PADDING_LEFT = 2;

    private TextView _hintTextView;
    private EditText _editText;

    private readonly Context _context;


    protected FloatLabeledEditText(IntPtr javaReference, JniHandleOwnership transfer)
      : base(javaReference, transfer)
    {
    }

    public FloatLabeledEditText(Context context)
      : base(context)
    {
      _context = context;
    }

    public FloatLabeledEditText(Context context, IAttributeSet attrs)
      : base(context, attrs)
    {
      _context = context;
      SetAttributes(attrs);
    }

    [TargetApi(Value = (int) BuildVersionCodes.Honeycomb)]
    public FloatLabeledEditText(Context context, IAttributeSet attrs, int defStyleAttr)
      : base(context, attrs, defStyleAttr)
    {
      _context = context;
      SetAttributes(attrs);
    }

    public FloatLabeledEditText(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
      : base(context, attrs, defStyleAttr, defStyleRes)
    {
    }

    private void SetAttributes(IAttributeSet attrs)
    {
      _hintTextView = new TextView(_context);

      var a = _context.ObtainStyledAttributes(attrs, Resource.Styleable.FloatLabeledEditText);

      var padding = a.GetDimensionPixelSize(Resource.Styleable.FloatLabeledEditText_fletPadding, 0);
      var defaultPadding =
        (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, DEFAULT_PADDING_LEFT, Resources.DisplayMetrics);
      var paddingLeft = a.GetDimensionPixelSize(Resource.Styleable.FloatLabeledEditText_fletPaddingLeft, defaultPadding);
      var paddingTop = a.GetDimensionPixelSize(Resource.Styleable.FloatLabeledEditText_fletPaddingTop, 0);
      var paddingRight = a.GetDimensionPixelSize(Resource.Styleable.FloatLabeledEditText_fletPaddingRight, 0);
      var paddingBottom = a.GetDimensionPixelSize(Resource.Styleable.FloatLabeledEditText_fletPaddingBottom, 0);
      var background = a.GetDrawable(Resource.Styleable.FloatLabeledEditText_fletBackground);
      var useAccentColor = a.GetBoolean(Resource.Styleable.FloatLabeledEditText_fletUseAccentColor, false);
      
      if (padding != 0)
      {
        _hintTextView.SetPadding(padding, padding, padding, padding);
      }
      else
      {
        _hintTextView.SetPadding(paddingLeft, paddingTop, paddingRight, paddingBottom);
      }

      if (background != null)
      {
        setHintBackground(background);
      }

      _hintTextView.SetTextAppearance(_context,
        a.GetResourceId(Resource.Styleable.FloatLabeledEditText_fletTextAppearance,
          Android.Resource.Style.TextAppearanceSmall));

      if (useAccentColor)
      {
        Color? fletAccentColor;
        if (fetchAccentColor(out fletAccentColor) && fletAccentColor.HasValue)
          _hintTextView.SetTextColor(fletAccentColor.Value);
      }

      //Start hidden
      _hintTextView.Visibility = ViewStates.Invisible;
      //AnimatorProxy.wrap(mHintTextView).setAlpha(0);

      AddView(_hintTextView, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

      a.Recycle();
    }

    [SuppressLint(Value = new[] {"NewApi"})]
    private void setHintBackground(Drawable background)
    {
      if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
      {
        _hintTextView.Background = background;
      }
      else
      {
        _hintTextView.SetBackgroundDrawable(background);
      }
    }

    private bool fetchAccentColor(out Color? color)
    {
      color = null;
      try
      {
        //Stock Material colorAccent
        var ident = _context.Resources.GetIdentifier("android:colorAccent", "attr", null);

        if (ident == 0)
          //AppCompat Material colorAccent
          ident = _context.Resources.GetIdentifier("colorAccent", "attr", null);

        if (ident == 0)
          return false;

        var a = _context.ObtainStyledAttributes(new[] { ident });

        color = a.GetColor(0, 0);

        a.Recycle();

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public override void AddView(View child, int index, ViewGroup.LayoutParams @params)
    {
      if (child.GetType() == typeof (EditText))
      {
        if (_editText != null)
        {
          throw new IllegalArgumentException("Can only have one EditText subview");
        }

        var lp = new LayoutParams(@params)
        {
          Gravity = GravityFlags.Bottom,
          TopMargin = (int) (_hintTextView.TextSize + _hintTextView.PaddingBottom + _hintTextView.PaddingTop)
        };
        @params = lp;

        setEditText((EditText) child);
      }


      base.AddView(child, index, @params);
    }


    private void setEditText(EditText editText)
    {
      _editText = editText;

      _editText.AfterTextChanged += delegate(object sender, AfterTextChangedEventArgs args)
      {
        setShowHint(!TextUtils.IsEmpty(args.Editable));
      };

      _editText.BeforeTextChanged += delegate(object sender, TextChangedEventArgs args)
      {
        
      };

      _editText.TextChanged += delegate(object sender, TextChangedEventArgs args)
      {
        
      };
      
      _editText.FocusChange += delegate(object sender, FocusChangeEventArgs args)
      {
        if (args.HasFocus && _hintTextView.Visibility == ViewStates.Visible)
        {
          ObjectAnimator.OfFloat(_hintTextView, "alpha", 0.33f, 1f).Start();
        }
        else if (_hintTextView.Visibility == ViewStates.Visible)
        {
          //AnimatorProxy.wrap(mHintTextView).setAlpha(1f); //Need this for compat reasons
          ObjectAnimator.OfFloat(_hintTextView, "alpha", 1f, 0.33f).Start();
        }
      };
     

      _hintTextView.SetText(_editText.Hint, TextView.BufferType.Normal);

      if (!string.IsNullOrEmpty(_editText.Text))
      {
        _hintTextView.Visibility = ViewStates.Visible;
      }
    }


    private void setShowHint(bool show)
    {
      AnimatorSet animation = null;

      if ((_hintTextView.Visibility == ViewStates.Visible) && !show)
      {
        animation = new AnimatorSet();
        var move = ObjectAnimator.OfFloat(_hintTextView, "translationY", 0, _hintTextView.Height/8);
        var fade = ObjectAnimator.OfFloat(_hintTextView, "alpha", 1, 0);
        animation.PlayTogether(move, fade);
      }
      else if ((_hintTextView.Visibility != ViewStates.Visible) && show)
      {
        animation = new AnimatorSet();
        var move = ObjectAnimator.OfFloat(_hintTextView, "translationY", _hintTextView.Height/8, 0);
        var fade = ObjectAnimator.OfFloat(_hintTextView, "alpha", 0, _editText.IsFocused ? 1 : 0.33f);
        animation.PlayTogether(move, fade);
      }

      if (animation == null) 
        return;

      animation.AnimationStart += delegate
      {
        base.OnAnimationStart(); 
        _hintTextView.Visibility = ViewStates.Visible;
      };

      animation.AnimationEnd += delegate
      {
        base.OnAnimationEnd();
        _hintTextView.Visibility = show ? ViewStates.Visible : ViewStates.Invisible;
        //AnimatorProxy.wrap(_hintTextView).setAlpha(show ? 1 : 0);
      };

      animation.Start();
    }

    public EditText getEditText()
    {
      return _editText;
    }

    public void setHint(string hint)
    {
      _editText.Hint = hint;
      _hintTextView.Text = hint;
    }

    public ICharSequence getHint()
    {
      return new Java.Lang.String(_hintTextView.Hint);
    }
  }
}