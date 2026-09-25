Feature: Signing in and out

  As someone who keeps a wardrobe in Buckl
  I want my garments behind my own account
  So that nobody else can see what I own

  Scenario: a visitor who is not signed in is welcomed instead of seeing a wardrobe
    Given nobody is signed in
    When the visitor opens the wardrobe
    Then the welcome screen is shown
    And a way to log in is offered

  Scenario: logging in comes back to the page the visitor asked for
    Given nobody is signed in
    And the visitor opened the address "/wardrobe/g-42?from=share"
    When the visitor chooses "Log in"
    Then the sign-in starts, asking to come back to "/wardrobe/g-42?from=share"

  Scenario: a failed sign-in is explained on the welcome screen
    Given the last sign-in attempt failed
    When the visitor opens the wardrobe
    Then the welcome screen explains that the sign-in did not work
    And a way to log in is offered

  Scenario: the app waits for the session instead of showing the welcome screen
    Given the session is still being restored
    When the visitor opens the wardrobe
    Then a loading message is shown
    And the welcome screen is not shown

  Scenario: a signed-in user sees the wardrobe and who they are
    Given Alice is signed in
    When she opens the wardrobe
    Then the wardrobe is shown
    And her name is shown in the header

  Scenario: a signed-in user who opens the welcome screen goes to the wardrobe
    Given Alice is signed in
    When she opens the welcome screen
    Then the wardrobe is shown

  Scenario: logging out ends the session
    Given Alice is signed in
    And she is on the wardrobe
    When she chooses "Log out"
    Then the session is ended

  Scenario: the sign-in callback waits for the session
    Given the session is still being restored
    When Auth0 sends the visitor back to the app
    Then a message says the sign-in is being completed

  Scenario: a sign-in that failed at Auth0 comes back to the welcome screen
    Given the last sign-in attempt failed
    When Auth0 sends the visitor back to the app
    Then the welcome screen explains that the sign-in did not work
