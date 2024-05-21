using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using System.Linq.Expressions;
using RadzenDialogService = Radzen.DialogService;

namespace MemoDown.Services
{
    public class DialogService
    {

        private readonly RadzenDialogService _dialogService;

        public DialogService(RadzenDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public Task<bool?> ConfirmDelete(string? content = null)
        {
            return _dialogService.Confirm($"确定删除{content}吗？", "提示", new ConfirmOptions()
            {
                OkButtonText = "确定",
                CancelButtonText = "取消",
                CloseDialogOnEsc = false,
                ShowClose = false,
                CloseDialogOnOverlayClick = false,
            });
        }

        public async Task<string?> ShowNamingDialog(string operation = "新建", string category = "笔记")
        {
            var title = $"{operation}{category}";
            var validationMsg = $"请输入{category}名称！";
            var name = string.Empty;

            RenderFragment<RadzenDialogService> dialogComponent = ds => __builder2 =>
            {
                __builder2.OpenComponent<RadzenTemplateForm<string>>(629);
                __builder2.AddComponentParameter(630, "Data", RuntimeHelpers.TypeCheck(name));
                __builder2.AddComponentParameter(631, "Submit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create<string>((object)this, (Action)delegate
                {
                    ds.Close(name);
                })));
                __builder2.AddAttribute(632, "ChildContent", (RenderFragment<EditContext>)((EditContext context) => delegate (RenderTreeBuilder __builder3)
                {
                    __builder3.OpenComponent<RadzenStack>(633);
                    __builder3.AddComponentParameter(634, "Orientation", RuntimeHelpers.TypeCheck(Orientation.Vertical));
                    __builder3.AddComponentParameter(635, "Gap", "1rem");
                    __builder3.AddComponentParameter(636, "JustifyContent", RuntimeHelpers.TypeCheck(JustifyContent.SpaceBetween));
                    __builder3.AddAttribute(637, "ChildContent", (RenderFragment)delegate (RenderTreeBuilder __builder4)
                    {
                        __builder4.OpenComponent<RadzenStack>(638);
                        __builder4.AddComponentParameter(639, "Orientation", RuntimeHelpers.TypeCheck(Orientation.Horizontal));
                        __builder4.AddComponentParameter(640, "Style", "height:54px");
                        __builder4.AddAttribute(641, "ChildContent", (RenderFragment)delegate (RenderTreeBuilder __builder5)
                        {
                            __builder5.OpenComponent<RadzenText>(642);
                            __builder5.AddComponentParameter(643, "Text", "名称：");
                            __builder5.AddComponentParameter(644, "Style", "padding-top:var(--rz-input-padding)");
                            __builder5.CloseComponent();
                            __builder5.AddMarkupContent(645, "\r\n                ");
                            __builder5.OpenComponent<RadzenStack>(646);
                            __builder5.AddComponentParameter(647, "Orientation", RuntimeHelpers.TypeCheck(Orientation.Vertical));
                            __builder5.AddComponentParameter(648, "Gap", "0");
                            __builder5.AddComponentParameter(649, "class", "flex-grow-1");
                            __builder5.AddAttribute(650, "ChildContent", (RenderFragment)delegate (RenderTreeBuilder __builder6)
                            {
                                __builder6.OpenComponent<RadzenTextBox>(651);
                                __builder6.AddComponentParameter(652, "Name", "Name");
                                __builder6.AddComponentParameter(653, "Value", RuntimeHelpers.TypeCheck(name));
                                __builder6.AddComponentParameter(654, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate (string __value)
                                {
                                    name = __value;
                                }, name))));
                                __builder6.AddComponentParameter(655, "ValueExpression", RuntimeHelpers.TypeCheck<Expression<Func<string>>>(() => name));
                                __builder6.CloseComponent();
                                __builder6.AddMarkupContent(656, "\r\n                    ");
                                __builder6.OpenComponent<RadzenRequiredValidator>(657);
                                __builder6.AddComponentParameter(658, "Component", "Name");
                                __builder6.AddComponentParameter(659, "Text", RuntimeHelpers.TypeCheck(validationMsg));
                                __builder6.CloseComponent();
                            });
                            __builder5.CloseComponent();
                        });
                        __builder4.CloseComponent();
                        __builder4.AddMarkupContent(660, "\r\n            ");
                        __builder4.OpenComponent<RadzenStack>(661);
                        __builder4.AddComponentParameter(662, "Orientation", RuntimeHelpers.TypeCheck(Orientation.Horizontal));
                        __builder4.AddComponentParameter(663, "JustifyContent", RuntimeHelpers.TypeCheck(JustifyContent.End));
                        __builder4.AddComponentParameter(664, "AlignItems", RuntimeHelpers.TypeCheck(AlignItems.Center));
                        __builder4.AddAttribute(665, "ChildContent", (RenderFragment)delegate (RenderTreeBuilder __builder5)
                        {
                            __builder5.OpenComponent<RadzenButton>(666);
                            __builder5.AddComponentParameter(667, "Text", "确定");
                            __builder5.AddComponentParameter(668, "ButtonType", RuntimeHelpers.TypeCheck(ButtonType.Submit));
                            __builder5.AddComponentParameter(669, "Style", "width: 80px;");
                            __builder5.CloseComponent();
                            __builder5.AddMarkupContent(670, "\r\n                ");
                            __builder5.OpenComponent<RadzenButton>(671);
                            __builder5.AddComponentParameter(672, "Text", "取消");
                            __builder5.AddComponentParameter(673, "Click", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
                            {
                                ds.Close(false);
                            })));
                            __builder5.AddComponentParameter(674, "ButtonStyle", RuntimeHelpers.TypeCheck(ButtonStyle.Light));
                            __builder5.CloseComponent();
                        });
                        __builder4.CloseComponent();
                    });
                    __builder3.CloseComponent();
                }));
                __builder2.CloseComponent();
                __builder2.AddMarkupContent(675, "\r\n");
            };

            var result = await _dialogService.OpenAsync(title, dialogComponent, new DialogOptions
            {
                Width = "400px",
                CloseDialogOnEsc = false,
                ShowClose = false,
                Height = "210px"
            });

            if (result?.GetType() == typeof(string))
            {
                return result as string;
            }
            return null;
        }
    }
}
