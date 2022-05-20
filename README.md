# CryptoWalletAuthenticator
This repository is for a school project where I researched an "emerging trend". The emerging trend I have been researching is blockchain. The main question of my research is; "How can I create an application that allows users to authenticate themselves with their personal blockchain wallet address?" 

To answer this research question, I created this project. It is an application that allows users to log in with their blockchain wallet address. Then they can add a favourite colour for their own address. 

The purpose of this application is the authentication of users. Users can only add a favourite colour for their own address if they are correctly authenticated. 

## How to use
### Install MetaMask
To use this application, you must have [MetaMask](https://metamask.io/download/) installed as a browser extension. If you do not have this you will get an error.

![MetaMask not installed](/Frontend/screenshots/1_MetaMask_not_installed.png "MetaMask not installed")
### Homepage
If MetaMask is installed you arrive at the HomePage

![Homepage](/Frontend/screenshots/2_before_signing_in.png "Homepage")

You can now sign in by using the sign in button
### Sign in
After clicking the sing in button, you select the wallet you want to use to sign in 

![Selecting wallet](/Frontend/screenshots/3_selecting_wallet.png "Selecting wallet")

Now you are asked to sign a nonce (Number Once). In the backend, this signed nonce is used to check whether a user actually owns the crypto wallet address. You can read how this works [here](#how-it-works). 

![Signing nonce](/Frontend/screenshots/4_signing_nonce.png "Signing nonce")

After signing the nonce you are signend in. A JWT token is now stored as a cookie in your browser. 

![Signed in](/Frontend/screenshots/5_signed_in.png "Signed in")

### Saving your favorite colour
When you are correctly signed in, you can set a favorite colour for your crypto wallet address. You do that by filling in the input field and saving your changes. Your favorite colour is now saved in the backend, for everyone to see! (JWT authentication is used for this function)

![Saved favorite colour](/Frontend/screenshots/6_Authentication_succeded_saved_colour.png "Saved favorite colour")

### Sign out
You can sign out by hitting the Sign out button. You are now signed out. But your favorite colour is still saved.

![Signed out](/Frontend/screenshots/7_sign_out.png "Signed out")

### Unauthorized Error
After signing out you are not able to change your favorite colour anymore.

![Unauthorized Error](/Frontend/screenshots/8_Unauthorized_error.png "Unauthorized Error")

### Different account
Just to prove that this application supports multiple wallet addresses, below are screenshots, of adding a favorite colour, for another wallet address.

![Signing in to different account](/Frontend/screenshots/9_sign_in_different_account.png "Signing in to different account")

(after selecting the account a new nonce was signed, and a new JWT was stored, ofcourse!)

![Signed in to different](/Frontend/screenshots/10_signed_in_to_different_account.png "Signed in to different")

![Colour saved for different address](/Frontend/screenshots/11_different_account_different_colour.png "Colour saved for different address")

## how it works
![How is works](/how-it-works.png "How is works")