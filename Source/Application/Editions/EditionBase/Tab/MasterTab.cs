using pdfforge.PDFCreator.UI.Presentation.ServiceLocator;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.TabHelper;
using Prism.Regions;
using SimpleInjector;
using System.Collections.Generic;

namespace pdfforge.PDFCreator.Editions.EditionBase.Tab
{
    public class MasterTab<T> : ISettingsTab
        where T : class, ITabViewModel
    {
        private readonly string _tabItemsRegionName;
        private readonly string _contentRegionName;

        public IList<ISubTab> SubTabs { get; set; } = new List<ISubTab>();

        public MasterTab(string tabItemsRegion, string tabContentRegion)
        {
            _tabItemsRegionName = tabItemsRegion;
            _contentRegionName = tabContentRegion;
        }

        public void AddSubTab(ISubTab subTab)
        {
            SubTabs.Add(subTab);
        }

        public void Register(IRegionManager regionManager, IWhitelistedServiceLocator serviceLocator, string regionName)
        {
            regionManager.RegisterMasterTab<T>(
                _tabItemsRegionName,
                _contentRegionName,
                regionName,
                serviceLocator);

            var isFirstTab = true;
            foreach (var subTab in SubTabs)
            {
                subTab.Register(regionManager, serviceLocator, _tabItemsRegionName, _contentRegionName, isFirstTab);
                isFirstTab = false;
            }
        }

        public void RegisterNavigationViews(Container container)
        {
            foreach (var subTab in SubTabs)
            {
                subTab.RegisterNavigationViews(container);
            }
        }
    }
}
