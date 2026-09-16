Feature: Edit a garment

  As someone who mislabelled a garment or found its receipt
  I want to correct its details
  So that filters and outfits use the right information

  Scenario: the form starts with the garment's current details
    Given a blue top sized M bought for 45000 "ARS" on "2026-03-15" with the note "Bought in Madrid"
    When the user opens the edit screen of the blue top
    Then the form shows the category "Top", the color "Blue" and the size "M"
    And the form shows the price 45000 in "ARS" paid on "2026-03-15"
    And the form shows the note "Bought in Madrid"
    And the current photo is shown

  Scenario: changing the color updates the garment
    Given a blue top sized M
    When the user opens the edit screen of the blue top
    And changes the color to "Navy"
    And saves the changes
    Then the detail is titled "Navy top"
    And the wardrobe garment has the color "navy"

  Scenario: the detail offers to edit an active garment
    Given a blue top sized M
    When the user opens the detail of the blue top
    Then an edit link leads to the edit screen

  Scenario: an archived garment cannot be edited
    Given an archived blue top
    When the user opens the edit screen of the blue top
    Then a message says the garment is archived and must be restored first
    And a link back to the garment is offered

  Scenario: a garment that does not exist cannot be edited
    Given a wardrobe with no garments
    When the user opens the edit screen of a garment that does not exist
    Then a message says the garment is not in the wardrobe
