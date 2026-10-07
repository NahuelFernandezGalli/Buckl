Feature: Session expiry

  As someone who left Buckl open for a long time
  I want to be told when I have to log in again
  So that a failed save or an empty screen does not leave me guessing

  Scenario: an expired session offers to log in again and come back
    Given the session of the user expired while looking at their blue garments
    Then a notice says the session expired
    When the user chooses to log in again
    Then the login comes back to the blue garments

  Scenario: a valid session shows no notice
    Given the user is looking at their wardrobe
    Then no notice about the session is shown

  Scenario: a screen that cannot load does not repeat the notice
    Given the session of the user expired while the wardrobe was loading
    Then the notice is the only alert on the screen
    And there is nothing to try again
