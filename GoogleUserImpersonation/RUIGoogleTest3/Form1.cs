using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Http;
using static Pet.RUIGoogle.CookieGetter;
using Pet.RUIGoogle;
using System.Runtime.InteropServices;
using CefSharp;
using HtmlAgilityPack;
using static Pet.RUIGoogle.GoogleRequestTransformer;

namespace RUIGoogleTest3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            OAuth2Negotiator.InitializeBrowser();
        }

        static string randomBase64urlencoded(uint size)
        {
            return Pkce.GenerateCodeChallenge(Pkce.GenerateCodeVerifier(size));
        }

        OAuth2Negotiator nego;
        string startupUrl = "https://accounts.google.com/";
        async Task<string> GetCookies(string url)
        {
            var cookieManager = chromiumWebBrowser1.GetCookieManager();
            if (cookieManager == null)
                throw new Exception("Can't get cookies. Browser is null.");

            //TaskCookieVisitor tcv = new TaskCookieVisitor();
            List<CefSharp.Cookie> cookieList = await cookieManager.VisitUrlCookiesAsync(url, true);

            //List<CefSharp.Cookie> cookieList = await tcv.Task;

            string cookies = "";
            foreach (CefSharp.Cookie cookie in cookieList)
                cookies += $"{cookie.Name}={cookie.Value}; ";
            cookies = cookies.Remove(cookies.Length - 2);

            return cookies;
        }
        AuthAdviceInfo aaInfo = new AuthAdviceInfo()
        {
            client_state = randomBase64urlencoded(34),
            system_version = "12.4.7",
            device_name = "Pet",
            device_id = "39781BE0-2D80-4E52-8255-B33679D0B902",
            device_challenge_request = randomBase64urlencoded(144),
            device_model = "iPad4,4"
        };
        
        private async void button1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(CreateAuthAdviceContent(aaInfo));

            GoogleAccountInfo gaInfo;
            gaInfo.username = "dummy@gmail.com";
            gaInfo.password = "dummy";

            nego = await OAuth2Negotiator.Create(aaInfo, gaInfo/*, chromiumWebBrowser1*/);
            //nego.Login("", "");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chromiumWebBrowser1.Load("https://accounts.google.com/ServiceLogin?passive=1209600&continue=https%3A%2F%2Faccounts.google.com%2F&followup=https%3A%2F%2Faccounts.google.com%2F");
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine(await GetCookies(startupUrl));
            nego.Login("muahanglazada231@gmail.com", /*await GetCookies(startupUrl));*/
                "__Host-GAPS=1:E_jjpU2f_q79nktzS1uE2kfBJqokE0jDJofqVa9dx-TCBL-uL0GPGXXfIxzES4TUP4vs6uCXKXOEv1EBYQxQ720IM0dqbD63FwiW4oxbvYJ7aGFlq7Gi1Uct4Y4d4rhiMaa0-xFCC2m-Lg:-UwfcdR1AEHEqOf7; GEM=CgptaW51dGVtYWlkEPP-h4-TMA==; APISID=ozzj6mkff6MtvDdj/AJPmgxOv5Z4lOtcNx; HSID=A0JIm9OA2My8jQP7U; SAPISID=PSGxnDXcATX7uejW/AImPIBLv530TnKP4F; SID=Kgh8C1sUz6eJI-qavRhR3kZUnI3-CTX8fmtM_5rjKBcD11OoX9Dy8wdqYPV7QiBU_BZX9w.; SSID=ARQcg_bv8tlbu8g6I; __Secure-1PAPISID=PSGxnDXcATX7uejW/AImPIBLv530TnKP4F; __Secure-1PSID=Kgh8C1sUz6eJI-qavRhR3kZUnI3-CTX8fmtM_5rjKBcD11OoJ04DmzyUipiN9vXuoNYkKA.; __Secure-3PAPISID=PSGxnDXcATX7uejW/AImPIBLv530TnKP4F; __Secure-3PSID=Kgh8C1sUz6eJI-qavRhR3kZUnI3-CTX8fmtM_5rjKBcD11Oo1ns5oaJzfoKOT3-Qdqe_Uw.; LSID=doritos|lso|o.groups.google.com|o.mail.google.com|o.myactivity.google.com|s.VN|s.youtube:Kgh8C9lqJQEWUM-u0937I3Ot4gqe-5yWsrMKNhIT8YN6U9lMsOMmu0AYCmILJydr2nipuw.; __Host-1PLSID=doritos|lso|o.groups.google.com|o.mail.google.com|o.myactivity.google.com|s.VN|s.youtube:Kgh8C9lqJQEWUM-u0937I3Ot4gqe-5yWsrMKNhIT8YN6U9lMXO8MNBBr8p9poQuPVsgWQw.; __Host-3PLSID=doritos|lso|o.groups.google.com|o.mail.google.com|o.myactivity.google.com|s.VN|s.youtube:Kgh8C9lqJQEWUM-u0937I3Ot4gqe-5yWsrMKNhIT8YN6U9lM6B4b6mpZGiYBR_MPBfoCDA.; NID=511=oOSZMViPvPS0aZthx0d3jZ9YJz1JFfxARn4yvSaT9NM7pz51tTVHvoT8PHvLEI21GgZoPIIrbRiHbCLRxRQd-jkn08DzjDgwIOhmLJRZqWYHdoLZ73rI4nHmQ-Z1fTR_hFY-Wj1IF3wcjUGMas43MYrOgxYUuUC-VeDXeNj6wyHz3nN8n-3Ep5OS_L97GXWB-BJtXsFQntoXFtG0w2lvN1uJ-_wMUQq3ESSAkLx16AA-bz-s7DT2JXd9SZZwKBocEMDr_sKr7-ifomgCc2gWpdXcyG8ZrdhzXJKrydZZG9_ALSPB6Pehva_B1IsEZ6M_UeEcMhDJMJFM34sxILpKBftWdHY7F8hluVN62uoLqt10NmN4e9WGjmY_i6uSWqKV3nOxR9er240uLQyOQgNGHdt_BrVqa_rHA2oD2lgRX0wP1qgS_GfKINIc1YLHJcaXenQ-pX1VURSO2j9z2E8b9Qsp9FWVc1H45M1I7L6bcgFPnltu6SjMZ9NNX4pRUWCcVJcFb7SJNfijUA9yS2tlWHgyp4ZlxHFrlV86lAyZa7tryTVzEb00Q19MCekQE6zCv03Tj3ZxeVR9dP4rA5A; ACCOUNT_CHOOSER=AFx_qI6xdqtGZO7NQP0deLjmWnTwFcgCpIuk0_YIViN6Yw-h8fS4oldMcd4UZS2e7Y5_iIgJeQ6sdQYu6BYunUtv0XDTXAR6rG05_gn3yZ1EEUsZ_N0aIzFjJ90C5LO_86OY00zNaMk743JIR9MzQcdILz56BVrVr_8Igg6kVXucIFOuuhnNfeUOK-WKB4w9bsZ20xJaseZOPcxQmRuz7gAuwiFHrHBDWgHTN06bdAMey9IWqZylUk2SPHqpDL1UTRy-V9LOnMw3OQu3kYtviKR11szZuK-uEvo-RxmzT0dgQ0lYiziRaA2Rskwc8pO5GWHNhZA2f72M5PAfImR-o8-0Zn7f-psD0eooA-DhSON4z_bizgXKAGwUFpjYaNFsbKkoQaXCyrJzDnbRAfoyuPFrh3NndEyZIEYKtur_SDQwfK8kUUJSRiYgoK6A4pa5TS5S03HLIHH057KSHi5YZ5Q29MwlVm4YXM9w-rV8Kjtmy04Hmsu2ULF_b6cfVj5hKXgeCd2hUhJLnHeWaAtmhes5186wORiwI1FiWIMh8qsyaEEuRdXz8nmOH9-V5_Ne9ykVUSBnxtQiE_aKW-0-oXfHA7zuD1NqhLukiNkEJii2p3RidwLqqwYsWXGexSsg2AYfil1NweEJ9KTeajT6IGbDVIWaacJnzYM4aubk59iaY-C5nA_Sthl63D0HZINvIR0Ww_xci7mAeDRSyyP0Q1Ru1fS2rmOLbwJd-9FP4HQ-ukcFEOa_WYrP_gvyJSI8QVBxAP9U32fJeP5fU-Lz6pgFCpo5jY61RY6Po1_noOhw_6O6RZ3tVGs; ANID=AHWqTUkGjBn1gqz5da2o35l1raWYbieivbe38AMVsTGcxXCMBFjBjQmbvSiSK9pU; AEC=AVQQ_LDpHC-t0Bmq58LbFZNS9GNyA9HJs_B2vNF8zIU5mowwv8vQLRCTOlI; SEARCH_SAMESITE=CgQIj5UB; CONSENT=YES+VN.vi+20170402-16-0; SIDCC=AJi4QfGxT6HiBmp-irPqTgEVdgVBuoLRfk9-b46_hTFRdmQncloGKLG02xySmO_eaTifYak85g; __Secure-3PSIDCC=AJi4QfF66EqiUmewYed0S9aEph4KDc5OAnV2aGzfEJWGizWw7E4QjuVa8_ATzYT_j3Lw0O5TbWcZ");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            nego.baiter.autoResponder.Dispose();
            chromiumWebBrowser1.Dispose();
            nego.baiter.browser.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Debug.WriteLine(GoogleConstants.FReq10_4._2[0]);
            EmbeddedSetupQuery result = new EmbeddedSetupQuery()
            {
                @as = "Quan",
                auth_extension = "Petterpet",
                code_challenge = "idk",
                device_name = aaInfo.device_name,
                system_version = aaInfo.system_version,
                device_model = aaInfo.device_model
            };
            //Debug.WriteLine(GoogleRequestTransformer.CreateEmbeddedSetupReferer(result));
        }

    }
}
