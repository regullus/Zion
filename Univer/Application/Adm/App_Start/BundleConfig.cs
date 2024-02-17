namespace Sistema
{
    using System;
    using System.Configuration;
    using Sistema.Constants;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;

    using System.Globalization;
    using System.IO;
    using System.Linq;

    public static class BundleConfig
    {
        /// <summary>
        
        /// </summary>
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Enable Optimizations
            // Set EnableOptimizations to false for debugging. For more information,
            // Web.config file system.web/compilation[debug=true]
            // OR
            // BundleTable.EnableOptimizations = true;

            // Enable CDN usage. 
            // Note: that you can choose to remove the CDN if you are developing an intranet application.
            // Note: We are using Google's CDN where possible and then Microsoft if not available for better 
            //       performance (Google is more likely to have been cached by the users browser).
            // Note: that protocol (http:) is omitted from the CDN URL on purpose to allow the browser to choose the protocol.
            bundles.UseCdn = true;

            AddCss(bundles);
            AddJavaScript(bundles);

            BundleTable.EnableOptimizations = false;
        }

        private static void AddCss(BundleCollection bundles)
        {

            #region Variaveis

            string strTema = ConfigurationManager.AppSettings["Sistema"];
            string strPath = "~/Content/admin/css/themes/Default.css";

            switch (strTema)
            {
                case "19L":
                    strPath = "~/Content/admin/css/themes/19L.css";
                    break;
                case "Afiliex":
                    strPath = "~/Content/admin/css/themes/Afiliex.css";
                    break;
                case "MettaBull":
                    strPath = "~/Content/admin/css/themes/MettaBull.css";
                    break;
                case "Universol":
                    strPath = "~/Content/admin/css/themes/Universol.css";
                    break;
                case "AtivaBox":
                    strPath = "~/Content/admin/css/themes/AtivaBox.css";
                    break;
                case "MinersBits":
                    strPath = "~/Content/admin/css/themes/MinersBits.css";
                    break;
                case "Nextter":
                    strPath = "~/Content/admin/css/themes/Nextter.css";
                    break;
                case "Default":
                    strPath = "~/Content/admin/css/themes/Default.css";
                    break;
                case "ZionFalkol":
                    strPath = "~/Content/admin/css/themes/Falkol.css";
                    break;
                case "SolSolar":
                    strPath = "~/Content/admin/css/themes/XSinergia.css";
                    break;
                case "ZionJB":
                    strPath = "~/Content/admin/css/themes/ZionJB.css";
                    break;
                case "XSinergia":
                    strPath = "~/Content/admin/css/themes/XSinergia.css";
                    break;
            }

            #endregion

            #region Login

            bundles.Add(new Bundle("~/css/theme/login")
               //BEGIN GLOBAL MANDATORY STYLES
               .Include("~/Content/global/plugins/simple-line-icons/simple-line-icons.min.css")
               .Include("~/Content/global/plugins/bootstrap/css/bootstrap.min.css")
               .Include("~/Content/global/plugins/bootstrap-switch/css/bootstrap-switch.min.css")
               //END GLOBAL MANDATORY STYLES
               //BEGIN PAGE LEVEL PLUGINS
               .Include("~/Content/global/plugins/select2/css/select2.min.css")
               .Include("~/Content/global/plugins/select2/css/select2-bootstrap.min.css")
               //END PAGE LEVEL PLUGINS
               //BEGIN THEME GLOBAL STYLES
               .Include("~/Content/global/css/components.min.css")
               .Include("~/Content/global/css/plugins.min.css")
               //END THEME GLOBAL STYLES
               //BEGIN PAGE LEVEL STYLES
               .Include("~/Content/pages/css/login-3.min.css")
               //END PAGE LEVEL STYLES 
               //LAYOUT
               .Include("~/Content/custom/login.css")
               .Include("~/Content/site.css")
               .Include("~/Content/fontawesome/site.css")
               //END LAYOUT
               );

            #endregion

            #region Layout

            //BEGIN GLOBAL MANDATORY STYLES
            bundles.Add(new Bundle("~/css/theme/mandatory")
              .Include("~/Content/global/plugins/font-awesome/css/font-awesome.min.css")
              .Include("~/Content/global/plugins/simple-line-icons/simple-line-icons.min.css")
              .Include("~/Content/global/plugins/bootstrap/css/bootstrap.min.css")
              .Include("~/Content/global/plugins/bootstrap-switch/css/bootstrap-switch.min.css")
              .Include("~/Content/global/plugins/jalert/jAlert-v3.css")
              .Include("~/Content/global/plugins/jquery-ui/jquery-ui.min.css")
              );

            //BEGIN THEME STYLES
            bundles.Add(new Bundle("~/css/theme/style")
               .Include("~/Content/global/css/components.min.css")
               .Include("~/Content/global/css/plugins.min.css")
               .Include("~/Content/layouts/layout4/css/layout.min.css")
               .Include("~/Content/layouts/layout4/css/themes/default.min.css")
               .Include("~/Content/layouts/layout4/css/custom.min.css")
               );
            //END THEME STYLES

            //BEGIN THEME TYPE
            bundles.Add(new Bundle("~/css/theme/type")
               .Include(strPath)
               );
            //END THEME TYPE

            #endregion

            #region Custom

            //BEGIN CUSTOM
            bundles.Add(new Bundle("~/Content/css")
               .Include("~/Content/site.css"));
            //END CUSTOM

            // BEGIN fontawesome - Icons using font (http://fortawesome.github.io/Font-Awesome/).
            bundles.Add(new StyleBundle("~/Content/fa", ContentDeliveryNetwork.MaxCdn.FontAwesomeUrl)
                .Include("~/Content/fontawesome/site.css"));
            //END fontawesome

            #endregion

            #region locais

            //Pagina de Home
            bundles.Add(new Bundle("~/Content/home", ContentDeliveryNetwork.MaxCdn.FontAwesomeUrl)
               .Include("~/Content/global/plugins/jplot/jquery.jqplot.css")
               );

            //Pagina MeusDados
            bundles.Add(new Bundle("~/Content/meusdados", ContentDeliveryNetwork.MaxCdn.FontAwesomeUrl)
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.css")
               );

            //Pagina Cadastro
            bundles.Add(new Bundle("~/Content/cadastro", ContentDeliveryNetwork.MaxCdn.FontAwesomeUrl)
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.css")
               .Include("~/Content/global/plugins/select2/select2.min.css")
               .Include("~/Content/global/plugins/bootstrap-markdown/css/bootstrap-markdown.min.css")
               .Include("~/Content/global/plugins/bootstrap-wysihtml5/bootstrap-wysihtml5.css")
               .Include("~/Content/global/plugins/bootstrap-datepicker/css/bootstrap-datepicker3.min.css")
               );

            //Pagina Detalhes Usuario
            bundles.Add(new Bundle("~/Content/detalhes", ContentDeliveryNetwork.MaxCdn.FontAwesomeUrl)
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.css")
               .Include("~/Content/admin/pages/css/profile.min.css")
               );

            //Agendamento
            bundles.Add(new Bundle("~/Content/agendamento", ContentDeliveryNetwork.MaxCdn.FontAwesomeUrl)
               .Include("~/Content/global/plugins/clockface/css/clockface.css")
               .Include("~/Content/global/plugins/bootstrap-datepicker/css/bootstrap-datepicker3.min.css")
               .Include("~/Content/global/plugins/bootstrap-timepicker/css/bootstrap-timepicker.min.css")
               .Include("~/Content/global/plugins/bootstrap-colorpicker/css/colorpicker.css")
               .Include("~/Content/global/plugins/bootstrap-daterangepicker/daterangepicker-bs3.css")
               .Include("~/Content/global/plugins/bootstrap-datetimepicker/css/bootstrap-datetimepicker.min.css")
               );

            #endregion

            #region Componentes

            //sweetAlert
            bundles.Add(new Bundle("~/Content/sweetAlert")
               .Include("~/Content/global/plugins/bootstrap-sweetalert/sweetalert.css")
            );

            //dataPiker
            bundles.Add(new Bundle("~/Content/dataPiker")
               .Include("~/Content/global/plugins/JQueryUICustom/jquery-ui.css")
            );

            //Crop image
            bundles.Add(new Bundle("~/Content/crop")
               .Include("~/Content/global/plugins/jcrop/css/jquery.Jcrop.min.css")
               );

            //Calendario
            bundles.Add(new Bundle("~/Content/calendario")
               .Include("~/Content/global/plugins/fullcalendar/fullcalendar.min.css")
            );

            //FileUpload
            bundles.Add(new Bundle("~/Content/fileupload")
               .Include("~/Content/global/plugins/jquery-file-upload/blueimp-gallery/blueimp-gallery.min.css")
               .Include("~/Content/global/plugins/jquery-file-upload/css/jquery.fileupload.css")
               .Include("~/Content/global/plugins/jquery-file-upload/css/jquery.fileupload-ui.css")
            );

            //Table
            bundles.Add(new Bundle("~/Content/table")
              .Include("~/Content/global/plugins/select2/select2.min.css")
              .Include("~/Content/global/plugins/datatables/extensions/Scroller/css/dataTables.scroller.min.css")
              .Include("~/Content/global/plugins/datatables/extensions/ColReorder/css/dataTables.colReorder.min.css")
              .Include("~/Content/global/plugins/datatables/plugins/bootstrap/dataTables.bootstrap.css")
              .Include("~/Content/admin/pages/css/tasks.css")
              .Include("~/Content/global/css/components.min.css")
           );

            //TableLeft
            bundles.Add(new Bundle("~/Content/tableLeft")
              .Include("~/Content/admin/css/tabLeft.css")
           );


            //tableSort
            bundles.Add(new Bundle("~/Content/tableSort")
               .Include("~/Content/global/plugins/tablesorter/css/jquery.tablesorter.pager.min.css")
           );

            //Rede
            bundles.Add(new Bundle("~/Content/rede")
               //.Include("~/Content/rede/demo/js/jquery/ui-lightness/jquery-ui-1.10.2.custom.css")
               .Include("~/Content/rede/demo/css/primitives.latest.css")
               //.Include("~/Content/rede/demo/jquerylayout/layout-default-latest.css")
               .Include("~/Content/rede/layout-default-latest.css")
           );

            //datatables
            bundles.Add(new Bundle("~/Content/datatables")
             .Include("~/Content/global/plugins/datatables/datatables.min.css")
             .Include("~/Content/global/plugins/datatables/plugins/bootstrap/datatables.bootstrap.css")
            );

            //dateragepicker
            bundles.Add(new Bundle("~/Content/dateRangePicker")
             .Include("~/Content/global/plugins/bootstrap-daterangepicker/daterangepicker.min.css")             
            );
            #endregion

        }

        /// <summary>
        /// Creates and adds JavaScript bundles to the bundle collection. Content Delivery Network's (CDN) are used 
        /// where available. 
        /// 
        /// Note: MVC's built in <see cref="System.Web.Optimization.Bundle.CdnFallbackExpression"/> is not used as 
        /// using in-line scripts is not permitted under Content Security Policy (CSP) (See <see cref="FilterConfig"/> 
        /// for more details).
        /// 
        /// Instead, we create our own fail-over bundles. If a CDN is not reachable, the fail-over script loads the 
        /// local bundles instead. The fail-over script is only a few lines of code and should have a minimal impact, 
        /// although it does add an extra request (Two if the browser is IE8 or less). If you feel confident in the CDN 
        /// availability and prefer better performance, you can delete these lines.
        /// </summary>
        /// <param name="bundles">The bundles.</param>
        private static void AddJavaScript(BundleCollection bundles)
        {

            #region Variaveis

            string strTema = ConfigurationManager.AppSettings["Tema"];

            #endregion

            #region Login

            Bundle scrThemeLogin = new Bundle("~/scripts/theme/login")
               .Include("~/Content/global/plugins/jquery.min.js")
               .Include("~/Content/scripts/jquery.globalize/globalize.js")
               .Include("~/Content/global/plugins/jquery-migrate.min.js")
               .Include("~/Content/global/plugins/bootstrap/js/bootstrap.min.js")
               .Include("~/Content/global/plugins/jquery.blockui.min.js")
               .Include("~/Content/global/plugins/uniform/jquery.uniform.min.js")
               .Include("~/Content/global/plugins/js.cokie.min.js")
               .Include("~/Content/global/plugins/jquery-validation/js/jquery.validate.min.js")
               .Include("~/Content/global/plugins/select2/select2.min.js")
               .Include("~/Content/global/scripts/app.min.js")
               .Include("~/Content/admin/layout/scripts/layout.js")
               .Include("~/Content/scripts/jquery.globalize/jquery.validate.globalize.js") //My jQuery Validate extension which depends on Globalize
               .Include("~/Content/admin/pages/scripts/login.min.js");
            bundles.Add(scrThemeLogin);

            #endregion

            #region Layout

            //BEGIN CORE JQUERY
            Bundle scrThemeJquery = new Bundle("~/scripts/theme/jquery")
               .Include("~/Content/global/plugins/jquery.min.js")
               .Include("~/Content/global/plugins/jquery.textrotator.min.js")
               .Include("~/Content/scripts/jquery.globalize/globalize.js"); //The Globalize library
            bundles.Add(scrThemeJquery);
            //END CORE JQUERY

            //BEGIN CORE PLUGINS
            Bundle scrThemeCore = new Bundle("~/scripts/theme/core")
               .Include("~/Content/global/plugins/jquery-migrate.min.js")
               .Include("~/Content/global/plugins/jquery-ui/jquery-ui.min.js")
               .Include("~/Content/global/plugins/bootstrap/js/bootstrap.min.js")
               .Include("~/Content/global/plugins/bootstrap-hover-dropdown/bootstrap-hover-dropdown.min.js")
               .Include("~/Content/global/plugins/js.cokie.min.js")
               .Include("~/Content/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js")
               .Include("~/Content/global/plugins/jquery.blockui.min.js")
               .Include("~/Content/global/plugins/uniform/jquery.uniform.min.js")
               .Include("~/Content/global/plugins/bootstrap-switch/js/bootstrap-switch.min.js")
               .Include("~/Content/global/plugins/bootstrap-toastr/toastr.min.js")
               .Include("~/Content/scripts/jquery.globalize/jquery.validate.globalize.js")
               .Include("~/Content/global/plugins/jalert/jAlert-v3.min.js")
               .Include("~/Content/global/plugins/jalert/jAlert-functions.js")
               ;
            bundles.Add(scrThemeCore);
            //END CORE PLUGINS

            //BEGIN THEME GLOBAL SCRIPTS
            Bundle scrThemeGlobal = new Bundle("~/scripts/theme/themeglobal")
               .Include("~/Content/global/scripts/app.min.js")
               ;
            bundles.Add(scrThemeGlobal);
            //END BEGIN THEME GLOBAL SCRIPTS


            //BEGIN THEME LAYOUT SCRIPTS
            Bundle scrThemeLayout = new Bundle("~/scripts/theme/themelayout")
               .Include("~/Content/layouts/layout4/scripts/layout.min.js")
               .Include("~/Content/layouts/layout4/scripts/demo.min.js")
               .Include("~/Content/layouts/global/scripts/quick-sidebar.min.js")
               .Include("~/Content/layouts/global/scripts/quick-nav.min.js")
               ;
            bundles.Add(scrThemeLayout);
            //END THEME LAYOUT SCRIPTS

            //Create culture specific bundles which contain the JavaScript files that should be served for each culture
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                bundles.Add(new ScriptBundle("~/js-culture." + culture.Name).Include( //example bundle name would be "~/js-culture.en-GB"
                    DetermineCultureFile(culture, "~/Content/scripts/jquery.globalize/cultures/globalize.culture.{0}.js"),             //The Globalize locale-specific JavaScript file
                    DetermineCultureFile(culture, "~/Content/scripts/jquery.globalize/bootstrap-datepicker-locales/bootstrap-datepicker.{0}.js") //The Bootstrap Datepicker locale-specific JavaScript file
                ));
            }

            #endregion

            #region Custom

            //BEGIN CUSTOM
            Bundle failoverCoreBundle = new Bundle("~/scripts/custom")
               .Include("~/Content/scripts/custom.js");
            bundles.Add(failoverCoreBundle);
            //END CUSTOM

            #endregion

            #region locais

            //Pagina Propaganda
            Bundle propagandaBundle = new Bundle("~/scripts/propaganda")
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.js")
               ;
            bundles.Add(propagandaBundle);

            //Pagina Home
            Bundle HomeBundle = new Bundle("~/scripts/home")
               .Include("~/Content/global/plugins/jplot/jquery.jqplot.js")
               .Include("~/Content/global/plugins/jplot/jqplot.funnelRenderer.js")
               ;
            bundles.Add(HomeBundle);

            //Pagina MeusDados
            Bundle meusDadosBundle = new Bundle("~/scripts/meusDados")
               .Include("~/Content/global/plugins/jquery-validation/js/jquery.validate.min.js")
               .Include("~/Content/global/plugins/forms/jquery.form.min.js")
               .Include("~/Content/global/plugins/forms/jquery.maskedinput.min.js")
               .Include("~/Content/global/plugins/jquery-steps/jquery.steps.min.js")
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.js")
            ;
            bundles.Add(meusDadosBundle);

            //Pagina Cadastro
            Bundle cadastroBundle = new Bundle("~/scripts/cadastro")
               .Include("~/Content/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js")
               .Include("~/Content/global/plugins/jquery-validation/js/jquery.validate.min.js")
               .Include("~/Content/global/plugins/jquery-validation/js/additional-methods.min.js")
               .Include("~/Content/global/plugins/forms/jquery.form.min.js")
               .Include("~/Content/global/plugins/forms/jquery.maskedinput.min.js")
               .Include("~/Content/global/plugins/jquery-steps/jquery.steps.min.js")
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.js")
               .Include("~/Content/global/plugins/select2/select2.min.js")
               .Include("~/Content/global/plugins/ckeditor/ckeditor.js")
               .Include("~/Content/global/plugins/bootstrap-markdown/js/bootstrap-markdown.js")
               .Include("~/Content/global/plugins/bootstrap-markdown/lib/markdown.js")
               .Include("~/Content/global/plugins/bootstrap-datepicker/js/bootstrap-datepicker.min.js")
               .Include("~/Content/global/plugins/bootstrap-wysihtml5/wysihtml5-0.3.0.js")
               .Include("~/Content/admin/pages/scripts/form-validation.min.js")
               .Include("~/Content/global/plugins/jquery-ui/jquery-ui.min.js") /*Importante Não remover - bug com o bootstrap*/
            ;
            bundles.Add(cadastroBundle);

            //Pagina Detalhes Usuario
            Bundle detalhesBundle = new Bundle("~/scripts/detalhes")
               .Include("~/Content/global/plugins/bootstrap-fileinput/bootstrap-fileinput.js")
               .Include("~/Content/global/plugins/jquery.sparkline.min.js")
               .Include("~/Content/global/plugins/jquery-inputmask/jquery.inputmask.bundle.min.js")
            ;
            bundles.Add(detalhesBundle);

            //Agendamento - outras linguas: https://github.com/eternicode/bootstrap-datepicker/tree/master/js/locales
            //ToDo - FAZER: tratar locales conforme linguagem do usuario
            Bundle agendamentoBundle = new Bundle("~/scripts/agendamento")
               .Include("~/Content/global/plugins/bootstrap-datepicker/js/bootstrap-datepicker.min.js")
               .Include("~/Content/global/plugins/bootstrap-datepicker/js/locales/bootstrap-datepicker.pt-BR.min.js")
               .Include("~/Content/global/plugins/bootstrap-timepicker/js/bootstrap-timepicker.min.js")
               .Include("~/Content/global/plugins/clockface/js/clockface.js")
               .Include("~/Content/global/plugins/bootstrap-daterangepicker/moment.min.js")
               .Include("~/Content/global/plugins/bootstrap-daterangepicker/daterangepicker.js")
               .Include("~/Content/global/plugins/bootstrap-colorpicker/js/bootstrap-colorpicker.js")
               .Include("~/Content/global/plugins/bootstrap-datetimepicker/js/bootstrap-datetimepicker.min.js")
               .Include("~/Content/admin/pages/scripts/components-pickers.js")
            ;
            bundles.Add(agendamentoBundle);

            #endregion

            #region Componentes
            //sweetalert
            Bundle sweetAlertBundle = new Bundle("~/scripts/sweetAlert")
               .Include("~/Content/global/plugins/bootstrap-sweetalert/sweetalert.min.js")
               .Include("~/Content/pages/scripts/ui-sweetalert.min.js")
            ;
            bundles.Add(sweetAlertBundle);

            //DataPicker
            Bundle dataPickerBundle = new Bundle("~/scripts/dataPiker")
               .Include("~/Content/global/plugins/JQueryUICustom/jquery-ui.js")
               .Include("~/Content/global/plugins/jquery-inputmask/jquery.inputmask.bundle.min.js")
            ;
            bundles.Add(dataPickerBundle);

            //Crop Image
            Bundle cropBundle = new Bundle("~/scripts/crop")
               .Include("~/Content/admin/pages/scripts/form-image-crop.js")
               .Include("~/Content/global/plugins/jcrop/js/jquery.color.js")
               .Include("~/Content/global/plugins/jcrop/js/jquery.Jcrop.min.js")
               ;
            bundles.Add(cropBundle);

            //Calendario
            Bundle dataCalendario = new Bundle("~/scripts/calendario")
               .Include("~/Content/global/plugins/moment.min.js")
               .Include("~/Content/global/plugins/fullcalendar/fullcalendar.min.js")
            ;
            bundles.Add(dataCalendario);

            //FileUpload
            Bundle dataFileUpload = new Bundle("~/scripts/fileupload")
               .Include("~/Content/global/plugins/fancybox/source/jquery.fancybox.pack.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/vendor/jquery.ui.widget.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/vendor/tmpl.min.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/vendor/load-image.min.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/vendor/canvas-to-blob.min.js")
               .Include("~/Content/global/plugins/jquery-file-upload/blueimp-gallery/jquery.blueimp-gallery.min.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.iframe-transport.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload-process.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload-image.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload-audio.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload-video.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload-validate.js")
               .Include("~/Content/global/plugins/jquery-file-upload/js/jquery.fileupload-ui.js")
            ;
            bundles.Add(dataFileUpload);

            //Table
            Bundle dataTable = new Bundle("~/scripts/table")
               .Include("~/Content/global/plugins/select2/select2.min.js")
               .Include("~/Content/global/plugins/datatables/media/js/jquery.dataTables.min.js")
               .Include("~/Content/global/plugins/datatables/extensions/TableTools/js/dataTables.tableTools.min.js")
               .Include("~/Content/global/plugins/datatables/extensions/ColReorder/js/dataTables.colReorder.min.js")
               .Include("~/Content/global/plugins/datatables/extensions/Scroller/js/dataTables.scroller.min.js")
               .Include("~/Content/global/plugins/datatables/plugins/bootstrap/dataTables.bootstrap.js")
            ;
            bundles.Add(dataTable);

            //TableSort
            Bundle dataTableSort = new Bundle("~/scripts/tableSort")
               .Include("~/Content/global/plugins/tablesorter/js/jquery.tablesorter.min.js")
               .Include("~/Content/global/plugins/tablesorter/js/jquery.tablesorter.widgets.min.js")
               .Include("~/Content/global/plugins/tablesorter/js/extras/jquery.tablesorter.pager.min.js")
            ;
            bundles.Add(dataTableSort);

            //Rede
            Bundle dataRede = new Bundle("~/scripts/rede")
               .Include("~/Content/rede/jquery.layout-latest.js")
               .Include("~/Content/rede/demo/js/primitives.min.js")

               ;
            bundles.Add(dataRede);

            //BEGIN FLOT
            Bundle scrFlot = new Bundle("~/scripts/theme/flot")
               .Include("~/Content/global/plugins/flot/jquery.flot.min.js")
               .Include("~/Content/global/plugins/flot/jquery.flot.resize.min.js")
               .Include("~/Content/global/plugins/flot/jquery.flot.pie.min.js")
               .Include("~/Content/global/plugins/flot/jquery.flot.stack.min.js")
               .Include("~/Content/global/plugins/flot/jquery.flot.crosshair.min.js")
               .Include("~/Content/global/plugins/flot/jquery.flot.categories.min.js")
               ;
            bundles.Add(scrFlot);
            //END FLOT

            //Datatables
            Bundle dataDatatables = new Bundle("~/scripts/datatables")
               .Include("~/Content/global/scripts/datatable.js")
               .Include("~/Content/global/plugins/datatables/datatables.min.js")
               .Include("~/Content/global/plugins/datatables/plugins/bootstrap/datatables.bootstrap.js")
               ;
            bundles.Add(dataDatatables);

            //Datatables
            Bundle dataEditable = new Bundle("~/scripts/editable")
                .Include("~/Content/pages/scripts/table-datatables-editable.min.js")
               ;
            bundles.Add(dataEditable);

            //validationPlugin
            Bundle validationPlugin = new Bundle("~/scripts/validationPlugin")
               .Include("~/Content/global/plugins/jquery-validation/js/jquery.validate.min.js")
               .Include("~/Content/global/plugins/jquery-validation/js/additional-methods.min.js")
              ;
            bundles.Add(validationPlugin);

            //validationScript
            Bundle validationScript = new Bundle("~/scripts/validationScript")
               .Include("~/Content/pages/scripts/form-validation-md.min.js")
              ;
            bundles.Add(validationScript);

            Bundle datePicker = new Bundle("~/scripts/dataPicker")
                .Include("~/Content/global/plugins/moment.min.js")
                .Include("~/Content/global/plugins/bootstrap-daterangepicker/daterangepicker.min.js")
                .Include("~/Content/global/plugins/bootstrap-datepicker/js/bootstrap-datepicker.min.js")
                .Include("~/Content/global/plugins/bootstrap-timepicker/js/bootstrap-timepicker.min.js")
                .Include("~/Content/global/plugins/bootstrap-datetimepicker/js/bootstrap-datetimepicker.min.js")
                .Include("~/Content/global/plugins/clockface/js/clockface.js");
            bundles.Add(datePicker);

            Bundle dateimePickerComponent = new Bundle("~/scripts/dataTimePickerComponent")
                 .Include("~/Content/pages/scripts/components-date-time-pickers.min.js");
            bundles.Add(dateimePickerComponent);
            #endregion


        }

        /// <summary>
        /// Given the supplied culture, determine the most appropriate Globalize culture script file that should be served up
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="filePattern">a file pattern, eg "~/Scripts/globalize-cultures/globalize.culture.{0}.js"</param>
        /// <param name="defaultCulture">Default culture string to use (eg "en-GB") if one cannot be found for the supplied culture</param>
        /// <returns></returns>
        private static string DetermineCultureFile(CultureInfo culture,
            string filePattern,
            string defaultCulture = "pt-BR"
            )
        {
            //Determine culture - GUI culture for preference, user selected culture as fallback
            var regionalisedFileToUse = string.Format(filePattern, defaultCulture);

            //Try to pick a more appropriate regionalisation if there is one
            if (File.Exists(HttpContext.Current.Server.MapPath(string.Format(filePattern, culture.Name)))) //First try for a globalize.culture.en-GB.js style file
                regionalisedFileToUse = string.Format(filePattern, culture.Name);
            else if (File.Exists(HttpContext.Current.Server.MapPath(string.Format(filePattern, culture.TwoLetterISOLanguageName)))) //That failed; now try for a globalize.culture.en.js style file
                regionalisedFileToUse = string.Format(filePattern, culture.TwoLetterISOLanguageName);

            return regionalisedFileToUse;
        }

    }
}