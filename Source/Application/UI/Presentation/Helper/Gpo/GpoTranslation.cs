﻿using Translatable;

namespace pdfforge.PDFCreator.UI.Presentation.Helper.Gpo
{
    public class GpoTranslation : ITranslatable
    {
        public string SetByAdministrator { get; private set; } = "Set by Administrator";
        public string DisabledByAdministrator { get; private set; } = "Disabled by Administrator";
        public string DisabledByAdministratorHint { get; private set; } = "These settings have been set by your administrator and can't be changed.";
    }
}
