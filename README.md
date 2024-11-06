# GoogleUserImpersonation
A program with custom-made libraries and dedicated code base to automate the exploitation of Google SSO and steal full-access authentication keys. My best attempt at finding a Remote User Impersonation exploit in Google SSO. Finished on 6/2022.

# How it works
Briefly speaking, the exploit follows these main steps:
1. Program sends a malicious iOS *authadvice* request with a custom authentication callback URL schema
2. Program retrieves the login URL from the response, and proceeds to automate the login process with a dummy but working Google Account:
* Program loads the login URL onto a customized built-in Chromium browser, which modifies the requests and responses on the fly as programmed, the code of which is custom-made.
* During the interception of requests and automation of the login process, there is a crucial step to retrieve a bot-checking code to then be incorporated into the next request. For this process, another browser is loaded, via a programmed proxy, with a sequence of fake requests and responses to then end up with the present web page HTTP response. Then, a network handler in the Chromium browser is coded to intercept and drop a prompted outgoing request containing the bot-checking code, to then feed to the program to send its own modified request.
3. After a few more automation steps and on-the-fly tampering of requests and responses, a final *CheckCookie* URL containing the content of previous steps is produced.
4. The next step involves either loading this *CheckCookie* URL on the target iOS device, or using previously retrieved cookies of a user. The URL with lead to a redirect to a *programmaticauth* URL, ending in the redirect to the initial intended URL as made in step 1, with the query appended by the *programmaticauth* authentication key.
5. This key is then processed through multiple steps involving requests and responses by the program to then retrieve a final Google SSO authentication key, which can be used to generate authentication keys for all Google API Services.
6. As a bonus, the program supports the extension of the lifetime of authentication keys, effectively granting lifetime access.

# Credits
PetterPet (Ho Minh Quan)
