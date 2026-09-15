Feature: App shell and navigation

  As someone using Buckl on the phone or in the browser
  I want a stable frame with the main sections always at hand
  So that I can move between my wardrobe and adding garments without getting lost

  Scenario: opening the app lands on the wardrobe
    When the user opens the app
    Then the wardrobe is shown

  Scenario: the main navigation reaches the add garment screen
    Given the app is open on the wardrobe
    When the user chooses "Add garment" in the main navigation
    Then the add garment screen is shown

  Scenario: the main navigation returns to the wardrobe
    Given the app is open on the add garment screen
    When the user chooses "Wardrobe" in the main navigation
    Then the wardrobe is shown

  Scenario: an unknown address shows a way back
    When the user opens an address that does not exist
    Then a page not found message is shown
    And a link back to the wardrobe is offered
