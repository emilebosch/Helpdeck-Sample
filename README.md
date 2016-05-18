# Helpdeck Experiment

I dediced to throw some of my old C# work online. To see how i would solve
the same issues nowadays using the new .NET frameworks.

It probably doesn't run anymore but still pretty sweet as reference!

## Installing

First clone the repository

    git clone https://github.com/emilebosch/catalyst-helpdeck.git
  
Create a heroku app

    heroku create --stack cedar
    heroku add:config BUILDPACK_URL https://github.com/emilebosch/catalyst-buildpack.git
    
Push the app to heroku

    git push heroku master
    
Finally check whether the app works

    heroku open
    
And done!