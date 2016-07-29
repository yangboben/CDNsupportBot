using CDNSupport.testclass;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CDNSupport
{
    [Serializable]
    public class ChooseDialog : IDialog<string>
    {

        private List<string> option_provide;
        private IEnumerable<tableitem> allOptions;
        private List<PropertyInfo> attributes;
        private QuestionInfo question;
        private int offset;

        private Stack<int> offsets;
        private Stack<IEnumerable<tableitem>> options_track;

        
        public ChooseDialog(int offset, QuestionInfo question, List<PropertyInfo> attributes, IEnumerable<tableitem> allOptions, List<string> option_provide)
        {
            this.option_provide = option_provide;
            this.offset = offset;
            this.question = question;
            this.attributes = attributes;
            this.allOptions = allOptions;

            offsets = new Stack<int>();
            options_track = new Stack<IEnumerable<tableitem>>();

           
        }

    
        public async Task StartAsync(IDialogContext context) {

            
            await context.PostAsync(askquestion(context.MakeMessage()));
               context.Wait(MessageReceivedAsync);
               
        }

        public  async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {

           
            var message = await argument;
     


            string receive;           
            if (!getreceive(out receive, message.Text)) {

                await context.PostAsync(askquestion(context.MakeMessage()));
                context.Wait(MessageReceivedAsync);
                return;
            }

            //TODO: 先判断是否是选项，若不是则跳出将这个发给最外层
           /* if (receive == "finish") {
                context.Done<string>("");
                return;
            }*/
            

            if (receive == "back")
            {

                int temp = offsets.Pop();
                var tempoptions = options_track.Pop();
                offset = temp == null ? offset : temp;
                allOptions = tempoptions == null ? allOptions : tempoptions;

            }
            offsets.Push(offset);
            options_track.Push(allOptions);
            PropertyInfo attribute = attributes[offset];

            attribute.SetValue(question, message.Text);
            
            bool if_find = true;
          
          
            while (offset < attributes.Count)
            {
                attribute = attributes[offset];


                if (!question.select<tableitem>(attribute.Name, (string)attribute.GetValue(question), allOptions, out allOptions, out option_provide))
                {
                    
                    if (allOptions.Count() == 1)
                    {      
                        //TODO 一个问题结束
                        if_find = true;
   
                        break;

                    }

                    if (option_provide.Count() < 1)
                    {
                        //无可用的回答，转人工服务
                        if_find = false;
                        await context.PostAsync("请联系客服");

                    }
                    else if (option_provide.Count() == 1)
                    {
                        //只有一个选项供选择
                        attribute.SetValue(question, option_provide[0]);
                        question.select(attribute.Name, (string)attribute.GetValue(question), allOptions, out allOptions, out option_provide);
                    }
                    else
                    {
                        //有多个选项 
                        if_find = false;                        
                        break;
                    }
                } 
                if (allOptions.Count() == 1)
                {
                    if_find = true;
                    break;
                }
                offset++;
            }

            if (if_find)
            {
                foreach (var i in allOptions)
                {
                    await context.PostAsync(answer(context.MakeMessage(),i.answer)).ConfigureAwait(false);
                }
                await context.PostAsync("您还有什么其他问题么").ConfigureAwait(false);
                //context.Wait(MessageReceivedAsync);c
                context.Done<string>("");
            }
            else {
                await context.PostAsync(askquestion(context.MakeMessage()));
                context.Wait(MessageReceivedAsync);
            }
          
        }


        public IMessageActivity askquestion(IMessageActivity message)
        {
           
            
            message.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
                       
            foreach(string i in option_provide){
                addbutton(cardButtons, i, i);
            }      

            HeroCard plCard = new HeroCard()
            {
                Title = getprompt(),
                Subtitle ="请直接回答选项",
                Buttons = cardButtons
            };


            List<CardAction> backbuttons = new List<CardAction>();
            backbuttons.Add(new CardAction(){
                Type = "postBack",
                Title = "输入back返回上一层",
                Value = "back"
            });

            HeroCard backCard = new HeroCard()
            {
                Buttons = backbuttons
            };


            
            message.Attachments.Add(plCard.ToAttachment());
            message.Attachments.Add(backCard.ToAttachment());
        
            return message;

        }

        //re是实际回复的内容string，content是显示给用户的
        public void addbutton(List<CardAction> cardButtons,string content, string re) {

            CardAction button = new CardAction()
            {
                Type = "postBack",
                Title = content,
                Value = re

            };
            cardButtons.Add(button);
       
        }


        public Boolean getreceive(out string result, string receive) {

            if (receive.Trim().ToLower() == "back")
            {
                result = "back";
                return true;
            }

            if (option_provide.Where(p => p == receive).Count() != 1)
            {
                result = null;
                return false;
            }
            else {

                result = receive;
                return true;
            }     
        }

        public IMessageActivity answer(IMessageActivity message, string answer) {

            message.Text = answer;
           /* message.Attachments = new List<Attachment>();

            List<CardAction> cardButtons = new List<CardAction>();
            addbutton(cardButtons, "我还有其他问题", "finish");
            addbutton(cardButtons, "输入back返回上一层", "back");
            
            HeroCard plCard = new HeroCard()
            {            
                Buttons = cardButtons
            };
            message.Attachments.Add(plCard.ToAttachment());
            */
            return message;
        }

        public string getprompt() { 
              
            switch(question.intent){
                case "create":
                    return "你想问的是关于创建_______的问题";
                case "deploy":
                    return "你想问的是关于创建_______的问题";
                case "price":
                    return "你想询问的是______的价格";
                case "troubleshooting":
                    return "你遇到的是______方面的问题";
                default:
                    return "something worng";
            }
        }
    }
}