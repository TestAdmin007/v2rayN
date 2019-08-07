using System;
using v2rayN.Handler;
using v2rayN.Mode;

namespace v2rayN.Forms
{

    public partial class LoginForm : v2rayN.Forms.BaseForm
    {

        /// <summary>
        /// 主窗体的引用
        /// </summary>
        public MainForm mainForm { get; set; }

        protected LoginCompletedData loginResult { get; set; }

        [Serializable]
        protected class LoginCompletedData
        {
            public string status { get; set; } = "";

            public string msg { get; set; } = "";

            public LoginSuccessData data { get; set; } = new LoginSuccessData();
        }

        [Serializable]
        public class LoginSuccessData
        {
            public string token { get; set; } = "";
        }

        public LoginForm()
        {
            InitializeComponent();

        }

        private void btn_login_click(object sender, EventArgs e)
        {
            var username = text_login_username.Text;

            if (Utils.IsNullOrEmpty(username)) {
                UI.Show(UIRes.I18N("MsgLoginEmptyUserName"));
                return;
            }

            var password = text_login_password.Text;

            if (Utils.IsNullOrEmpty(password)) {
                UI.Show(UIRes.I18N("MsgLoginEmptyPassword"));
                return;
            }

            if (password == "@HelloWorld@")
            {
                password = config.user.password;
            }
            else {
                password = Utils.md5(password.Trim()).ToUpper();
            }

            V2rayUpdateHandle LoginPost = new V2rayUpdateHandle();

            LoginPost.WebLogin("http://127.0.0.1/index.php", username, password);

            LoginPost.LoginCompleted += (serder2, args) =>
            {
                loginResult = Utils.FromJson<LoginCompletedData>(args.Msg);

                if (loginResult.status == "SUCCESS")
                {
                    //登录成功
                    if (config.user == null) {
                        config.user = new UserConfig();
                    }

                    config.user.username = username;
                    config.user.password = password;
                    config.user.token = loginResult.data.token;
                    ConfigHandler.ToJsonFile(config);
                    mainForm.tsbSubUpdate_Click(sender, e);
                    this.Close();
                    
                }
                else {
                    //登录失败
                    UI.Show(UIRes.I18N("MsgLoginUserNameAndPasswordNotMatch"));
                }
            };

            LoginPost.Error += (sender2, args) =>
            {
               UI.Show(args.GetException().Message);
            };
        }

    }
}
