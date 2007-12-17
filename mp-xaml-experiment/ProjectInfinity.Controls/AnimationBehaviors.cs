/*
Shared Source Community License (SS-CL)
Published: October 18, 2005

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction" and "distribution" have the same meaning
here as under U.S. copyright law.

"You" means the licensee of the software.

"Larger work" means the combination of the software and any additions or
modifications to the software.

"Licensed patents" means any Licensor patent claims which read directly on the
software as distributed by Licensor under this license.


2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, the Licensor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce the
software, prepare derivative works of the software and distribute the software
or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, the Licensor grants you a non-exclusive,
worldwide, royalty-free patent license under licensed patents to make, have
made, use, practice, sell, and offer for sale, and/or otherwise dispose of the
software or derivative works of the software.


3. Conditions and Limitations

(A) Reciprocal Grants- Your rights to reproduce and distribute the software (or
any part of the software), or to create and distribute derivative works of the
software, are conditioned on your licensing the software or any larger work you
create under the following terms:

1. If you distribute the larger work as a series of files, you must grant all
recipients the copyright and patent licenses in sections 2(A) & 2(B) for
any file that contains code from the software. You must also provide
recipients the source code to any such files that contain code from the
software along with a copy of this license. Any other files which are
entirely your own work and which do not contain any code from the software
may be licensed under any terms you choose.

2. If you distribute the larger work as a single file, then you must grant
all recipients the rights set out in sections 2(A) & 2(B) for the entire
larger work. You must also provide recipients the source code to the
larger work along with a copy of this license.

(B) No Trademark License- This license does not grant you any rights to use the 
Licensor’s name, logo, or trademarks.

(C) If you distribute the software in source code form you may do so only under
this license (i.e., you must include a complete copy of this license with your
distribution), and if you distribute the software solely in compiled or object
code form you may only do so under a license that complies with this license.

(D) If you begin patent litigation against the Licensor over patents that you
think may apply to the software (including a cross-claim or counterclaim in a
lawsuit), your license to the software ends automatically.

(E) The software is licensed "as-is." You bear the risk of using it. The Licensor
gives no express warranties, guarantees or conditions. You may have additional
consumer rights under your local laws which this license cannot change. To the
extent permitted under your local laws, the Licensor excludes the implied
warranties of merchantability, fitness for a particular purpose and
non-infringement. 
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ProjectInfinity.Controls
{

  #region Behavior Enumerations

  /// <summary>
  /// LoadedBehaviors are used to add an animation that runs when a page is loaded.  
  /// Currenly only one LoadedBehavior is supported per element however by using decorator elements multiple behaviors can be simultated
  /// </summary>
  public enum LoadedBehavior
  {
    None,
    FadeIn,
    FadeOut,
    ZoomIn,
    ZoomInSpringy,
    ZoomInRotate,
    SlideInFromLeft,
    SlideInFromTop,
    SlideInFromRight,
    SlideInFromBottom,
    ScaleInVertically,
    ScaleInHorizontally,
  }

  /// <summary>
  /// UnloadedBehaviors are used to add an animation that runs when a page navigated away from.  
  /// Currenly only one UnloadedBehavior is supported per element however by using decorator elements multiple behaviors can be simultated
  /// </summary>
  public enum UnloadedBehavior
  {
    None,
    FadeIn,
    FadeOut,
    ZoomOut,
    ZoomOutRotate,
    SlideOutToLeft,
    SlideOutToTop,
    SlideOutToRight,
    SlideOutToBottom,
    ScaleOutVertically,
    ScaleOutHorizontally,
  }

  /// <summary>
  /// ClickBehaviors are used to add an animation that runs when an element is clicked on.
  /// For most elements this actually means the MouseUp event, however for buttons it hooks to the Click event
  /// Currenly only one ClickBehavior is supported per element however by using decorator elements multiple behaviors can be simultated
  /// </summary>
  public enum ClickBehavior
  {
    None,
    Jiggle,
    Throb,
    Rotate,
    Snap,
  }

  /// <summary>
  /// LayoutBehaviors are used to add an animation that runs when an element gets a new layout position.
  /// Instead of simply poping to the new position LayoutBehaviors can cause the element to animate to the new position
  /// Currenly only one ClickBehavior is supported per element however by using decorator elements multiple behaviors can be simultated
  /// </summary>
  public enum LayoutBehavior
  {
    None,
    Smooth,
    Springy,
  }

  #endregion

  /// <summary>
  /// AnimationBehaviorHost is responsible for adding animations to elements that use it's attached properties.
  /// 
  /// This class is typically located near the top of each pages visual tree so that decendant elements can add
  /// common animations simply by attaching a property in XAML.
  /// 
  /// Currently behaviors do not attempt to preserve RenderTransforms, so any element that uses AnimationBehaviors should not
  /// count on a consistant TransoformCollection.
  /// </summary>
  public class AnimationBehaviorHost : Decorator
  {
    /// <summary>
    /// LoadedBehaviorProperty is an attached property which allows decendant elements to run animations when the page is loaded
    /// </summary>
    public static readonly DependencyProperty LoadedBehaviorProperty =
      DependencyProperty.RegisterAttached("LoadedBehavior",
                                          typeof (LoadedBehavior),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(LoadedBehavior.None, LoadedBehaviorChanged));

    /// <summary>
    /// LoadedDurationProperty is an attached property used to control the duration of the LoadedBehavior animation.  It defaults to 1 second.
    /// </summary>
    public static readonly DependencyProperty LoadedDurationProperty =
      DependencyProperty.RegisterAttached("LoadedDuration",
                                          typeof (Duration),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1))));

    /// <summary>
    /// LoadedDelayProperty is an attached property used to the duration of the delay before the LoadedBehavior animation starts.  It defaults to 0 seconds.
    /// </summary>
    public static readonly DependencyProperty LoadedDelayProperty =
      DependencyProperty.RegisterAttached("LoadedDelay",
                                          typeof (Duration),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(new Duration(TimeSpan.Zero)));

    /// <summary>
    /// UnloadedBehaviorProperty is an attached property which allows decendant elements to run animations when the page is navigated away from
    /// </summary>
    public static readonly DependencyProperty UnloadedBehaviorProperty =
      DependencyProperty.RegisterAttached("UnloadedBehavior",
                                          typeof (UnloadedBehavior),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(UnloadedBehavior.None));

    /// <summary>
    /// UnloadedDurationProperty is an attached property used to control the duration of the UnloadedBehavior animation.  It defaults to 1 second.
    /// </summary>
    public static readonly DependencyProperty UnloadedDurationProperty =
      DependencyProperty.RegisterAttached("UnloadedDuration",
                                          typeof (Duration),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1))));

    /// <summary>
    /// UnloadedDelayProperty is an attached property used to the duration of the delay before the UnloadedBehavior animation starts.  It defaults to 0 seconds.
    /// </summary>
    public static readonly DependencyProperty UnloadedDelayProperty =
      DependencyProperty.RegisterAttached("UnloadedDelay",
                                          typeof (Duration),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(new Duration(TimeSpan.Zero)));

    /// <summary>
    /// ClickBehaviorProperty is an attached property which allows decendant elements to run animations when the element is clicked.
    /// If the element is not a button the MouseUp event is used instead.
    /// </summary>
    public static readonly DependencyProperty ClickBehaviorProperty =
      DependencyProperty.RegisterAttached("ClickBehavior",
                                          typeof (ClickBehavior),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(ClickBehavior.None, ClickBehaviorChanged));

    /// <summary>
    /// ClickDurationProperty is an attached property used to control the duration of the ClickBehavior animation.  It defaults to 0.5 seconds.
    /// </summary>
    public static readonly DependencyProperty ClickDurationProperty =
      DependencyProperty.RegisterAttached("ClickDuration",
                                          typeof (Duration),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(500))));

    /// <summary>
    /// LayoutBehaviorProperty is an attached property that can be set on an element to control how it responds to being layed out
    /// </summary>
    public static readonly DependencyProperty LayoutBehaviorProperty =
      DependencyProperty.RegisterAttached("LayoutBehavior",
                                          typeof (LayoutBehavior),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(LayoutBehavior.None, LayoutBehaviorChanged));

    /// <summary>
    /// LayoutDurationProperty is an attached property used to control the duration of the LayoutBehavior animation.  It defaults to 0.5 seconds.
    /// </summary>
    public static readonly DependencyProperty LayoutDurationProperty =
      DependencyProperty.RegisterAttached("LayoutDuration",
                                          typeof (Duration),
                                          typeof (AnimationBehaviorHost),
                                          new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(500))));

    /// <summary>
    /// When a navigation is canceled we hold on to the arguments so we can resume the navigation after UnloadBehavior animations have completed
    /// </summary>
    private NavigatingCancelEventArgs canceledNavigation = null;

    /// <summary>
    /// this dictionary associates elements with the last known position relative to the host
    /// </summary>
    private readonly Dictionary<FrameworkElement, Point?> layoutBehaviorElementPosition =
      new Dictionary<FrameworkElement, Point?>();

    /// <summary>
    /// we remember how many elements need layout animations so that we can unsubscribe from the LayoutUpdated event when its not needed
    /// </summary>
    private int layoutBehaviorCount = 0;

    /// <summary>
    /// AnimationBehaviorHost constructor
    /// </summary>
    public AnimationBehaviorHost()
    {
      //hook loaded because we need the navigation service and it's not available at this time
      Loaded += AnimationBehaviorHost_Loaded;
    }

    #region Event Handlers

    /// <summary>
    /// AnimationBehaviorHost_Loaded is called when this class is Loaded because the NavigationService is not available at construction time.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AnimationBehaviorHost_Loaded(object sender, RoutedEventArgs e)
    {
      //hook the Navigating event so we can cancel it to allow UnloadedBehavior animations to run first
      NavigationService ns = NavigationService.GetNavigationService(this);
      if (ns != null)
      {
        ns.Navigating += CancelNavigating;
      }
    }

    /// <summary>
    /// When a 'click' is initiated we apply an aninimation to the element based on it's ClickBehavior type.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ApplyClickBehavior(object sender, RoutedEventArgs e)
    {
      FrameworkElement element = (FrameworkElement) sender;
      ClickBehavior behavior = GetClickBehavior(element);
      Duration duration = GetClickDuration(element);

      switch (behavior)
      {
        case ClickBehavior.Jiggle:
          ApplyJiggle(element, duration);
          break;
        case ClickBehavior.Throb:
          ApplyThrob(element, duration);
          break;
        case ClickBehavior.Rotate:
          ApplyRotate(element, duration);
          break;
        case ClickBehavior.Snap:
          ApplySnap(element, duration);
          break;
      }
    }

    /// <summary>
    /// This is where the LoadedBehavior animations takes place.  We simply apply an animation based on the type of behavior
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ApplyLoadedBehavior(object sender, RoutedEventArgs e)
    {
      FrameworkElement element = (FrameworkElement) sender;
      LoadedBehavior behavior = GetLoadedBehavior(element);
      Duration duration = GetLoadedDuration(element);
      Duration delay = GetLoadedDelay(element);

      switch (behavior)
      {
        case LoadedBehavior.FadeIn:
          ApplyFadeIn(element, duration, delay);
          break;
        case LoadedBehavior.FadeOut:
          ApplyFadeOut(element, duration, delay);
          break;
        case LoadedBehavior.ZoomIn:
          ApplyZoomIn(element, duration, delay);
          break;
        case LoadedBehavior.ZoomInSpringy:
          ApplyZoomInSpringy(element, duration, delay);
          break;
        case LoadedBehavior.ZoomInRotate:
          ApplyZoomInRotate(element, duration, delay);
          break;
        case LoadedBehavior.SlideInFromLeft:
          ApplySlideInFromLeft(element, duration, delay);
          break;
        case LoadedBehavior.SlideInFromTop:
          ApplySlideInFromTop(element, duration, delay);
          break;
        case LoadedBehavior.SlideInFromRight:
          ApplySlideInFromRight(element, duration, delay);
          break;
        case LoadedBehavior.SlideInFromBottom:
          ApplySlideInFromBottom(element, duration, delay);
          break;
        case LoadedBehavior.ScaleInVertically:
          ApplyScaleInVertically(element, duration, delay);
          break;
        case LoadedBehavior.ScaleInHorizontally:
          ApplyScaleInHorizontally(element, duration, delay);
          break;
      }
    }

    /// <summary>
    /// This event handler is called when the NavigationService fires it's Navigating event.
    /// We then cancel the event, run UnloadBehavior animations, and then resume the navigation once they are done
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelNavigating(object sender, NavigatingCancelEventArgs e)
    {
      List<FrameworkElement> unloadedBehaviorElements = GetUnloadBehaviorElements();
      if (unloadedBehaviorElements.Count > 0)
      {
        canceledNavigation = e;
        UnloadBehaviorsComplete += ResumeNavigation;
        ApplyUnloadedBehaviors(unloadedBehaviorElements);
        e.Cancel = true;
      }
    }

    /// <summary>
    /// this gets called whenever ANY element is updated so we need to check that a specific elements position has changed
    /// before adding animations.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLayoutUpdated(object sender, EventArgs e)
    {
      //TODO: rewrite this method to clean it up and handle corner cases with layout
      Dictionary<FrameworkElement, Point> updateDict = new Dictionary<FrameworkElement, Point>();
      foreach (KeyValuePair<FrameworkElement, Point?> pair in layoutBehaviorElementPosition)
      {
        FrameworkElement fe = pair.Key;
        Point? savedPosition = pair.Value;
        Point currentPosition = fe.TransformToAncestor(this).Transform(new Point(0, 0));
        if (savedPosition.HasValue)
        {
          if (!AreClose(currentPosition, savedPosition.Value))
          {
            LayoutBehavior behavior = GetLayoutBehavior(fe);
            Duration duration = GetLayoutDuration(fe);
            switch (behavior)
            {
              case LayoutBehavior.Smooth:
                ApplySmoothLayout(fe, savedPosition.Value, currentPosition, duration);
                break;
              case LayoutBehavior.Springy:
                ApplySpringyLayout(fe, savedPosition.Value, currentPosition, duration);
                break;
            }

            //this is probably a shitty way to update the collection
            updateDict[fe] = currentPosition;
          }
        }
        else
        {
          //the first time it's layed out just remember where
          updateDict[fe] = currentPosition;
        }
      }

      //update layoutBehaviorElementPosition now that we're not iterating it
        foreach (KeyValuePair<FrameworkElement, Point> pair in updateDict)
        {
          layoutBehaviorElementPosition[pair.Key] = new Point?(pair.Value);
        }
    }

    /// <summary>
    /// When a navigation is attempted we first cancel it then once UnloadBehavior animations have complete we resume the Navigation in this method
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResumeNavigation(object sender, EventArgs e)
    {
      UnloadBehaviorsComplete -= ResumeNavigation;
      NavigationService ns = NavigationService.GetNavigationService(this);
      if (ns != null)
      {
        ns.Navigating -= CancelNavigating; //we dont need to cancel next time around
        if (canceledNavigation.NavigationMode == NavigationMode.Back)
        {
          ns.GoBack();
        }
        else if (canceledNavigation.NavigationMode == NavigationMode.Forward)
        {
          ns.GoForward();
        }
        else if (canceledNavigation.Content != null)
        {
          //Navigate to previously stored object
          ns.Navigate(canceledNavigation.Content, canceledNavigation.ExtraData);
        }
        else
        {
          //Navigate to previously stored URI
          ns.Navigate(canceledNavigation.Uri, canceledNavigation.ExtraData);
        }
      }
    }

    #endregion

    /// <summary>
    /// Used to set an elements LoadedBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetLoadedBehavior(DependencyObject element, LoadedBehavior b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(LoadedBehaviorProperty, b);
    }

    /// <summary>
    /// Used to get an elements LoadedBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static LoadedBehavior GetLoadedBehavior(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (LoadedBehavior) element.GetValue(LoadedBehaviorProperty);
    }

    /// <summary>
    /// Used to set an elements LoadedBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetLoadedDuration(DependencyObject element, Duration b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(LoadedDurationProperty, b);
    }

    /// <summary>
    /// Used to get an elements LoadedBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    public static Duration GetLoadedDuration(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (Duration) element.GetValue(LoadedDurationProperty);
    }

    /// <summary>
    /// Used to set an elements LoadedDelay attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetLoadedDelay(DependencyObject element, Duration b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(LoadedDelayProperty, b);
    }

    /// <summary>
    /// Used to get an elements LoadedDelay attached property
    /// </summary>
    /// <param name="element"></param>
    public static Duration GetLoadedDelay(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (Duration) element.GetValue(LoadedDelayProperty);
    }

    #region Loaded Behavior Realization

    /// <summary>
    /// This gets called when the LoadedBehavior attached property is set on an element.  We simply hook it's Loaded
    /// event.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void LoadedBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement element = (d as FrameworkElement);
      if (element != null)
      {
        LoadedBehavior newbehavior = (LoadedBehavior) e.NewValue;
        LoadedBehavior oldbehavior = (LoadedBehavior) e.OldValue;

        if (newbehavior == oldbehavior)
        {
          return;
        }

        //walk the tree to find the closes AnimationBehaviorHost
        AnimationBehaviorHost host = FindHost(element);
        if (host == null)
        {
          return;
        }

        //dont forget to remove the event if the user decides to set it to None form something else
        if (newbehavior == LoadedBehavior.None)
        {
          element.Loaded -= host.ApplyLoadedBehavior;
        }

        //hook loaded if not None so we can apply the animation once it's on the page
        if (oldbehavior == LoadedBehavior.None)
        {
          element.Loaded += host.ApplyLoadedBehavior;
        }
      }
    }

    #endregion

    #region Loaded Behaviors Applied

    private static void ApplyFadeIn(IAnimatable fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0.0, 1.0, duration);
        fe.BeginAnimation(OpacityProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        fe.BeginAnimation(OpacityProperty, da);
      }
    }

    private static void ApplyFadeOut(IAnimatable fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(1.0, 0.0, duration);
        fe.BeginAnimation(OpacityProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        fe.BeginAnimation(OpacityProperty, da);
      }
    }

    private static void ApplyZoomIn(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0.0, 1.0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
    }

    private static void ApplyZoomInSpringy(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.05, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.95, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));

        da.Duration = duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;

        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.05, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.95, KeyTime.Paced));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));

        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;

        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
    }

    private static void ApplyZoomInRotate(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da1 = new DoubleAnimation(0.0, 1.0, duration);
        da1.AccelerationRatio = da1.DecelerationRatio = 0.2;

        DoubleAnimation da2 = new DoubleAnimation(0.0, 360.0, duration);
        da2.AccelerationRatio = da2.DecelerationRatio = 0.2;

        TransformGroup tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(1, 1));
        tg.Children.Add(new RotateTransform(0));

        tg.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, da1);
        tg.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, da1);

        tg.Children[1].BeginAnimation(RotateTransform.AngleProperty, da2);

        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = tg;
      }
      else
      {
        DoubleAnimationUsingKeyFrames da1 = new DoubleAnimationUsingKeyFrames();
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da1.Duration = delay + duration;
        da1.AccelerationRatio = da1.DecelerationRatio = 0.2;

        DoubleAnimationUsingKeyFrames da2 = new DoubleAnimationUsingKeyFrames();
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(360.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da2.Duration = delay + duration;
        da2.AccelerationRatio = da2.DecelerationRatio = 0.2;

        TransformGroup tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(1, 1));
        tg.Children.Add(new RotateTransform(0));

        tg.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, da1);
        tg.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, da1);

        tg.Children[1].BeginAnimation(RotateTransform.AngleProperty, da2);

        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = tg;
      }
    }

    private void ApplySlideInFromLeft(FrameworkElement fe, Duration duration, Duration delay)
    {
      GeneralTransform transform = fe.TransformToAncestor(this);
      Point slidepoint = transform.Transform(new Point(fe.ActualWidth, 0));

      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(-slidepoint.X, 0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(-slidepoint.X, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(-slidepoint.X, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
    }

    private void ApplySlideInFromTop(FrameworkElement fe, Duration duration, Duration delay)
    {
      GeneralTransform transform = fe.TransformToAncestor(this);
      Point slidepoint = transform.Transform(new Point(0, fe.ActualHeight));

      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(-slidepoint.Y, 0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(-slidepoint.Y, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(-slidepoint.Y, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
    }

    private void ApplySlideInFromRight(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(ActualWidth, 0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(ActualWidth, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(ActualWidth, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
    }

    private void ApplySlideInFromBottom(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(ActualHeight, 0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(ActualHeight, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(ActualHeight, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
    }

    private static void ApplyScaleInVertically(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0.0, 1.0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
    }

    private static void ApplyScaleInHorizontally(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0.0, 1.0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
      }
    }

    #endregion

    #region Unloaded Behavior

    #region Unloaded Behavior Attached Properties

    /// <summary>
    /// Used to set an elements UnloadedBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetUnloadedBehavior(DependencyObject element, UnloadedBehavior b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(UnloadedBehaviorProperty, b);
    }

    /// <summary>
    /// Used to get an elements UnloadedBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    public static UnloadedBehavior GetUnloadedBehavior(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (UnloadedBehavior) element.GetValue(UnloadedBehaviorProperty);
    }


    /// <summary>
    /// Used to set an elements UnloadedDuration attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetUnloadedDuration(DependencyObject element, Duration b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(UnloadedDurationProperty, b);
    }

    /// <summary>
    /// Used to get an elements UnloadedDuration attached property
    /// </summary>
    /// <param name="element"></param>
    public static Duration GetUnloadedDuration(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (Duration) element.GetValue(UnloadedDurationProperty);
    }

    /// <summary>
    /// Used to set an elements UnloadedDelay attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetUnloadedDelay(DependencyObject element, Duration b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(UnloadedDelayProperty, b);
    }

    /// <summary>
    /// Used to get an elements UnloadedDelay attached property
    /// </summary>
    /// <param name="element"></param>
    public static Duration GetUnloadedDelay(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (Duration) element.GetValue(UnloadedDelayProperty);
    }

    #endregion

    #region Unloaded Behavior Realization

    /// <summary>
    /// This even is fired after UnloadBehavior animations have completed but before the page is navigated
    /// </summary>
    public event EventHandler UnloadBehaviorsComplete;

    /// <summary>
    /// Walks the visual tree looking for FE's that have unload behaviors
    /// </summary>
    /// <returns></returns>
    private List<FrameworkElement> GetUnloadBehaviorElements()
    {
      List<FrameworkElement> unloadedBehaviorElements = new List<FrameworkElement>();

      GetUnloadBehaviorElementsRecursive(this, unloadedBehaviorElements);

      return unloadedBehaviorElements;
    }

    /// <summary>
    /// recursivly does a tree walk (depth first) to collect FE's
    /// </summary>
    /// <param name="fe"></param>
    /// <param name="list"></param>
    private static void GetUnloadBehaviorElementsRecursive(FrameworkElement fe, List<FrameworkElement> list)
    {
      if (GetUnloadedBehavior(fe) != UnloadedBehavior.None)
      {
        list.Add(fe);
      }

      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(fe); i++)
      {
        FrameworkElement child = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
        if (child != null)
        {
          GetUnloadBehaviorElementsRecursive(child, list);
        }
      }
    }

    /// <summary>
    /// Given a list of elements with Unloaded Behaviors this method simply applies them and waits till they are done
    /// </summary>
    /// <param name="unloadedBehaviorElements"></param>
    private void ApplyUnloadedBehaviors(IEnumerable<FrameworkElement> unloadedBehaviorElements)
    {
      TimeSpan longestUnload = TimeSpan.FromSeconds(0);
      //find the longest UnloadedBehavior so we can wait till it's done
      foreach (FrameworkElement fe in unloadedBehaviorElements)
      {
        ApplyUnloadedBehavior(fe);
        Duration dur = GetUnloadedDelay(fe) + GetUnloadedDuration(fe);
        if (dur.TimeSpan > longestUnload)
        {
          longestUnload = dur.TimeSpan;
        }
      }
      new DispatcherTimer(longestUnload, DispatcherPriority.Render, HandleUnloadedBehaviorComplete, Dispatcher.CurrentDispatcher);
    }

    /// <summary>
    /// Once all the UnloadedBehaviors have completed we should fire our UnloadBehaviorsComplete event
    /// to let anyone else know about this.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleUnloadedBehaviorComplete(object sender, EventArgs args)
    {
      DispatcherTimer unloadTimer = (DispatcherTimer) sender;
      unloadTimer.Stop();
      if (UnloadBehaviorsComplete != null)
      {
        UnloadBehaviorsComplete(this, null);
      }
    }

    /// <summary>
    /// Given an element, simply apply animations based on the type of UnloadedBehavior it has
    /// </summary>
    /// <param name="element"></param>
    private void ApplyUnloadedBehavior(FrameworkElement element)
    {
      UnloadedBehavior behavior = GetUnloadedBehavior(element);
      Duration duration = GetUnloadedDuration(element);
      Duration delay = GetUnloadedDelay(element);

      switch (behavior)
      {
        case UnloadedBehavior.FadeIn:
          ApplyFadeIn(element, duration, delay);
          break;
        case UnloadedBehavior.FadeOut:
          ApplyFadeOut(element, duration, delay);
          break;
        case UnloadedBehavior.ZoomOut:
          ApplyZoomOut(element, duration, delay);
          break;
        case UnloadedBehavior.ZoomOutRotate:
          ApplyZoomOutRotate(element, duration, delay);
          break;
        case UnloadedBehavior.SlideOutToLeft:
          ApplySlideOutToLeft(element, duration, delay);
          break;
        case UnloadedBehavior.SlideOutToTop:
          ApplySlideOutToTop(element, duration, delay);
          break;
        case UnloadedBehavior.SlideOutToRight:
          ApplySlideOutToRight(element, duration, delay);
          break;
        case UnloadedBehavior.SlideOutToBottom:
          ApplySlideOutToBottom(element, duration, delay);
          break;
        case UnloadedBehavior.ScaleOutVertically:
          ApplyScaleOutVertically(element, duration, delay);
          break;
        case UnloadedBehavior.ScaleOutHorizontally:
          ApplyScaleOutHorizontally(element, duration, delay);
          break;
      }
    }

    #endregion

    #region UnloadedBehaviors Applied

    private static void ApplyZoomOut(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(1.0, 0.0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
    }

    private static void ApplyZoomOutRotate(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da1 = new DoubleAnimation(1.0, 0.0, duration);
        da1.AccelerationRatio = da1.DecelerationRatio = 0.2;

        DoubleAnimation da2 = new DoubleAnimation(0.0, -360.0, duration);
        da2.AccelerationRatio = da2.DecelerationRatio = 0.2;

        TransformGroup tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(1, 1));
        tg.Children.Add(new RotateTransform(0));

        tg.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, da1);
        tg.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, da1);

        tg.Children[1].BeginAnimation(RotateTransform.AngleProperty, da2);

        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = tg;
      }
      else
      {
        DoubleAnimationUsingKeyFrames da1 = new DoubleAnimationUsingKeyFrames();
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da1.Duration = delay + duration;
        da1.AccelerationRatio = da1.DecelerationRatio = 0.2;

        DoubleAnimationUsingKeyFrames da2 = new DoubleAnimationUsingKeyFrames();
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(-360.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da2.Duration = delay + duration;
        da2.AccelerationRatio = da2.DecelerationRatio = 0.2;

        TransformGroup tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(1, 1));
        tg.Children.Add(new RotateTransform(0));

        tg.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, da1);
        tg.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, da1);

        tg.Children[1].BeginAnimation(RotateTransform.AngleProperty, da2);

        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = tg;
      }
    }


    private void ApplySlideOutToLeft(FrameworkElement fe, Duration duration, Duration delay)
    {
      GeneralTransform transform = fe.TransformToAncestor(this);
      Point slidepoint = transform.Transform(new Point(fe.ActualWidth, 0));

      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0, -slidepoint.X, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(
          new LinearDoubleKeyFrame(-slidepoint.X, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
    }

    private void ApplySlideOutToTop(FrameworkElement fe, Duration duration, Duration delay)
    {
      GeneralTransform transform = fe.TransformToAncestor(this);
      Point slidepoint = transform.Transform(new Point(0, fe.ActualHeight));

      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0, -slidepoint.Y, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(
          new LinearDoubleKeyFrame(-slidepoint.Y, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
    }

    private void ApplySlideOutToRight(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0, ActualWidth, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(ActualWidth, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da);
      }
    }

    private void ApplySlideOutToBottom(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(0, ActualHeight, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(
          new LinearDoubleKeyFrame(ActualHeight, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0, 0);
        fe.RenderTransform = new TranslateTransform(0, 0);
        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
      }
    }

    private static void ApplyScaleOutVertically(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(1.0, 0.0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
      }
    }

    private static void ApplyScaleOutHorizontally(UIElement fe, Duration duration, Duration delay)
    {
      if (delay.TimeSpan == TimeSpan.Zero)
      {
        DoubleAnimation da = new DoubleAnimation(1.0, 0.0, duration);
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
      }
      else
      {
        DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(delay.TimeSpan)));
        da.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(delay.TimeSpan + duration.TimeSpan)));
        da.Duration = delay + duration;
        da.AccelerationRatio = da.DecelerationRatio = 0.2;
        fe.RenderTransformOrigin = new Point(0.5, 0.5);
        fe.RenderTransform = new ScaleTransform(1, 1);
        fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
      }
    }

    #endregion

    #endregion

    #region Click Behavior

    #region Click Behavior Attached Properties

    /// <summary>
    /// Used to set an elements ClickBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetClickBehavior(DependencyObject element, ClickBehavior b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(ClickBehaviorProperty, b);
    }

    /// <summary>
    /// Used to get an elements ClickBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static ClickBehavior GetClickBehavior(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (ClickBehavior) element.GetValue(ClickBehaviorProperty);
    }

    /// <summary>
    /// Used to set an elements ClickDuration attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetClickDuration(DependencyObject element, Duration b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(ClickDurationProperty, b);
    }

    /// <summary>
    /// Used to get an elements ClickDuration attached property
    /// </summary>
    /// <param name="element"></param>
    public static Duration GetClickDuration(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (Duration) element.GetValue(ClickDurationProperty);
    }

    #endregion

    #region Click Behavior Realization

    /// <summary>
    /// When the ClickBehavior attached property is changed we subscribe to the appropriate event on the element
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void ClickBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement element = (d as FrameworkElement);
      if (element != null)
      {
        ClickBehavior newbehavior = (ClickBehavior) e.NewValue;
        ClickBehavior oldbehavior = (ClickBehavior) e.OldValue;

        if (newbehavior == oldbehavior)
        {
          return;
        }

        AnimationBehaviorHost host = FindHost(element);
        if (host == null)
        {
          return;
        }

        //use Click if it's a button, otherwise use MouseUp
        ButtonBase button =
          element as ButtonBase;
        if (button != null)
        {
          //use click handler
          if (newbehavior == ClickBehavior.None)
          {
            button.Click -= ApplyClickBehavior;
          }

          if (oldbehavior == ClickBehavior.None)
          {
            button.Click += ApplyClickBehavior;
          }
        }
        else
        {
          //no click handler, so fall back to mouse up
          if (newbehavior == ClickBehavior.None)
          {
            element.MouseUp -= ApplyClickBehavior;
          }

          if (oldbehavior == ClickBehavior.None)
          {
            element.MouseUp += ApplyClickBehavior;
          }
        }
      }
    }

    #endregion

    #region Click Behaviors Applied

    private static void ApplyJiggle(UIElement fe, Duration duration)
    {
      DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(10, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(-10, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));

      da.Duration = duration;
      da.AccelerationRatio = da.DecelerationRatio = 0.2;

      fe.RenderTransformOrigin = new Point(0.5, 0.5);
      fe.RenderTransform = new RotateTransform(0);
      fe.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, da);
    }

    private static void ApplyThrob(UIElement fe, Duration duration)
    {
      DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1.1, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1.05, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0.95, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));

      da.Duration = duration;
      da.AccelerationRatio = da.DecelerationRatio = 0.2;

      fe.RenderTransformOrigin = new Point(0.5, 0.5);
      fe.RenderTransform = new ScaleTransform(1, 1);
      fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, da);
      fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
    }

    private static void ApplyRotate(UIElement fe, Duration duration)
    {
      DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(90, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(180, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(270, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(360, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(365, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(360, KeyTime.Paced));

      da.Duration = duration;
      da.AccelerationRatio = da.DecelerationRatio = 0.2;

      fe.RenderTransformOrigin = new Point(0.5, 0.5);
      fe.RenderTransform = new RotateTransform(0);
      fe.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, da);
    }

    private static void ApplySnap(UIElement fe, Duration duration)
    {
      DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.Paced));
      da.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.Paced));

      da.Duration = duration;
      da.AccelerationRatio = da.DecelerationRatio = 0.2;

      fe.RenderTransformOrigin = new Point(0.5, 0.5);
      fe.RenderTransform = new ScaleTransform(1, 1);
      fe.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, da);
    }

    #endregion

    #endregion

    #region Layout Behavior

    #region Layout Behavior Attached Properties

    /// <summary>
    /// sets the LayoutBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetLayoutBehavior(DependencyObject element, LayoutBehavior b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(LayoutBehaviorProperty, b);
    }

    /// <summary>
    /// gets the LayoutBehavior attached property
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static LayoutBehavior GetLayoutBehavior(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (LayoutBehavior) element.GetValue(LayoutBehaviorProperty);
    }

    /// <summary>
    /// Used to set an elements LayoutDuration attached property
    /// </summary>
    /// <param name="element"></param>
    /// <param name="b"></param>
    public static void SetLayoutDuration(DependencyObject element, Duration b)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      element.SetValue(LayoutDurationProperty, b);
    }

    /// <summary>
    /// Used to get an elements LayoutDuration attached property
    /// </summary>
    /// <param name="element"></param>
    public static Duration GetLayoutDuration(DependencyObject element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return (Duration) element.GetValue(LayoutDurationProperty);
    }

    #endregion

    #region Layout Behavior Realization

    /// <summary>
    /// called when an element changes it's LayoutBehavior.  Here we simply remember which elements require
    /// layout animations so that later when layout is updated we cant add animations as nessesary.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void LayoutBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement element = (d as FrameworkElement);
      if (element != null)
      {
        LayoutBehavior newbehavior = (LayoutBehavior) e.NewValue;
        LayoutBehavior oldbehavior = (LayoutBehavior) e.OldValue;

        if (newbehavior == oldbehavior)
        {
          return;
        }

        AnimationBehaviorHost host = FindHost(element);
        if (host == null)
        {
          return;
        }

        if (oldbehavior == LayoutBehavior.None)
        {
          host.RegisterLayoutBehaviorElement(element);
        }

        if (newbehavior == LayoutBehavior.None)
        {
          host.UnregisterLayoutBehaviorElement(element);
        }
      }
    }

    private int LayoutBehaviorCount
    {
      get { return layoutBehaviorCount; }
      set
      {
        int oldval = layoutBehaviorCount;
        layoutBehaviorCount = value;
        if (oldval == 0 && layoutBehaviorCount > 0)
        {
          LayoutUpdated += OnLayoutUpdated;
        }
        else if (oldval > 0 && layoutBehaviorCount == 0)
        {
          LayoutUpdated -= OnLayoutUpdated;
        }
      }
    }

    private void RegisterLayoutBehaviorElement(FrameworkElement element)
    {
      LayoutBehaviorCount++;
      layoutBehaviorElementPosition[element] = null;
    }

    private void UnregisterLayoutBehaviorElement(FrameworkElement element)
    {
      LayoutBehaviorCount--;
      layoutBehaviorElementPosition.Remove(element);
    }

    #endregion

    #region Layout Behaviors Applied

    private static void ApplySmoothLayout(UIElement fe, Point oldpoint, Point newpoint, Duration duration)
    {
      fe.RenderTransform = new TranslateTransform();
      DoubleAnimation da1 = new DoubleAnimation(oldpoint.X - newpoint.X, 0.0, duration);
      da1.AccelerationRatio = da1.DecelerationRatio = 0.2;
      DoubleAnimation da2 = new DoubleAnimation(oldpoint.Y - newpoint.Y, 0.0, duration);
      da2.AccelerationRatio = da2.DecelerationRatio = 0.2;
      fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da1);
      fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da2);
    }

    private static void ApplySpringyLayout(UIElement fe, Point oldpoint, Point newpoint, Duration duration)
    {
      fe.RenderTransform = new TranslateTransform();
      if (oldpoint.X != newpoint.X)
      {
        double startx = oldpoint.X - newpoint.X;
        double dx = -startx;

        DoubleAnimationUsingKeyFrames da1 = new DoubleAnimationUsingKeyFrames();
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(startx, KeyTime.Paced));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(startx + dx*1.25, KeyTime.Paced));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(startx + dx*0.75, KeyTime.Paced));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(startx + dx*1.1, KeyTime.Paced));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(startx + dx*0.9, KeyTime.Paced));
        da1.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.Paced));

        da1.Duration = duration;
        da1.AccelerationRatio = da1.DecelerationRatio = 0.2;

        fe.RenderTransform.BeginAnimation(TranslateTransform.XProperty, da1);
      }

      if (oldpoint.Y != newpoint.Y)
      {
        double starty = oldpoint.Y - newpoint.Y;
        double dy = -starty;

        DoubleAnimationUsingKeyFrames da2 = new DoubleAnimationUsingKeyFrames();
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(starty, KeyTime.Paced));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(starty + dy*1.25, KeyTime.Paced));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(starty + dy*0.75, KeyTime.Paced));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(starty + dy*1.1, KeyTime.Paced));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(starty + dy*0.9, KeyTime.Paced));
        da2.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.Paced));

        da2.Duration = duration;
        da2.AccelerationRatio = da2.DecelerationRatio = 0.2;


        fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da2);
      }
    }

    #endregion

    #endregion

    #region Utility Methods

    /// <summary>
    /// a utility method to see if two points are close together
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    private static bool AreClose(Point p1, Point p2)
    {
      return (Math.Abs(p1.X - p2.X) < .001 && Math.Abs(p1.Y - p2.Y) < .001);
    }


    /// <summary>
    /// Given an element, this method walks the tree to find the AnimationBehaviorHost
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private static AnimationBehaviorHost FindHost(FrameworkElement e)
    {
      FrameworkElement current = e;

      while (current != null)
      {
        AnimationBehaviorHost host = current as AnimationBehaviorHost;

        if (host != null)
        {
          return host;
        }

        current = VisualTreeHelper.GetParent(current) as FrameworkElement;
      }

      return null;
    }

    #endregion
  }
}