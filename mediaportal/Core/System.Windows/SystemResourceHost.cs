#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace System.Windows
{
  public class SystemResourceHost
  {
    //http://msdn.microsoft.com/windowsvista/default.aspx?pull=/library/en-us/dnlong/html/wpf101.asp

/*
 * 
 * 
<DockPanel xmlns="http://schemas.microsoft.com/winfx/avalon/2005" 
xmlns:x="http://schemas.microsoft.com/winfx/xaml/2005" Margin="10">
   <DockPanel.Resources>
      <XmlDataProvider x:Key="Blog" Source="http://blogs.msdn.com/tims/rss.aspx" />

      <DataTemplate x:Key="TitleTemplate">
         <TextBlock Text="{Binding XPath=title}"/>
      </DataTemplate>
   </DockPanel.Resources>

   <Label Content="{Binding Source={StaticResource Blog}, XPath=/rss/channel/title}" 
FontSize="24" FontWeight="Bold" DockPanel.Dock="Top" />      
   
   <Label Content="{Binding Source={StaticResource Blog}, XPath=/rss/channel/description}" FontSize="18" DockPanel.Dock="Top" />

   <DockPanel DataContext="{Binding Source={StaticResource Blog}, XPath=/rss/channel/item}" >
      <ListBox DockPanel.Dock="Left" ItemsSource="{Binding}" ItemTemplate="{StaticResource TitleTemplate}" IsSynchronizedWithCurrentItem="True" />
      <Frame Source="{Binding XPath=link}" Width="Auto" />
   </DockPanel>
</DockPanel>

*/

    public SystemResourceHost()
    {
      //
      // TODO: Add constructor logic here
      //
    }
  }
}