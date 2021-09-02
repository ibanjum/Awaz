namespace Shot.Models
{
    public class PopupModel
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string ButtonText { get; set; }

        public PopupModel(string title, string text, string buttonText)
        {
            Title = title;
            Text = text;
            ButtonText = buttonText;
        }
    }
}
