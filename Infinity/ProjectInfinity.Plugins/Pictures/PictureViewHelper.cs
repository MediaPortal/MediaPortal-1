using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Pictures
{
  public static class PictureViewHelper
  {
    public static PictureViewModel GetViewModel()
    {
      return ServiceScope.Get<PictureViewModel>();
    }
  }
}
