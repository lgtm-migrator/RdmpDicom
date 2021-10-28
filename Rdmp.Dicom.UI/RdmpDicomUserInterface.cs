﻿using System.Drawing;
using ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Dicom.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Dicom.ExternalApis;
using Rdmp.Core.CommandExecution;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;
using System.Linq;

namespace Rdmp.Dicom.UI
{
    public class RdmpDicomUserInterface : PluginUserInterface, IRefreshBusSubscriber
    {
        IActivateItems _activator;

        public RdmpDicomUserInterface(IBasicActivateItems itemActivator) : base(itemActivator)
        {
            _activator = itemActivator as IActivateItems;
        }

        public override IEnumerable<IAtomicCommand> GetAdditionalRightClickMenuItems(object o)
        {
            if(_activator == null)
            {
                return Enumerable.Empty<IAtomicCommand>();
            }
            //IMPORTANT: if you are creating a menu array for a class in your own plugin instead create it as a Menu (See TagPromotionConfigurationMenu)

            var databaseEntity = o as DatabaseEntity;

            //allow clicking in Catalogue collection whitespace
            if (o is RDMPCollection collection && collection == RDMPCollection.Catalogue)
            {
                return new[] { new ExecuteCommandCreateNewImagingDataset(_activator) };
            }

            switch (databaseEntity)
            {
                case Catalogue c:
                    return new IAtomicCommand[] {
                        new ExecuteCommandCreateNewImagingDataset(_activator),
                        new ExecuteCommandPromoteNewTag(_activator).SetTarget(databaseEntity),
                        new Rdmp.Dicom.CommandExecution.ExecuteCommandCreateNewSemEHRCatalogue(_activator),
                        new ExecuteCommandCompareImagingSchemas(_activator,c)
                        };
                        
                case ProcessTask pt:
                    return new[] { new ExecuteCommandReviewIsolations(_activator, pt) };
                case TableInfo _:
                    return new[] { new ExecuteCommandPromoteNewTag(_activator).SetTarget(databaseEntity) };
            }

            return o is AllExternalServersNode ? new[] { new ExecuteCommandCreateNewExternalDatabaseServer(_activator, new SMIDatabasePatcher(), PermissableDefaults.None) } : new IAtomicCommand[0];
        }

        public override object[] GetChildren(object model)
        {
            return null;
        }

        public override Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            return null;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public override bool CustomActivate(IMapsDirectlyToDatabaseTable o)
        {
            if(_activator == null)
            {
                return false;
            }

            if(o is AggregateConfiguration ac)
            {
                var api = new SemEHRApiCaller();

                if (api.ShouldRun(ac))
                {
                    var ui = new SemEHRUI(_activator, api, ac);
                    ui.ShowDialog();
                    return true;
                }
            }

            return base.CustomActivate(o);
        }
    }
}
