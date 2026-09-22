Feature: Add a garment

  As someone who just bought or found a garment
  I want to add it to my wardrobe with a photo and a few facts
  So that it shows up when I decide what to wear

  Background:
    Given the user is on the add garment screen

  Scenario: a garment with a photo and its classification is saved
    When the user takes a photo of the garment
    And chooses the category "Top" and the color "Blue"
    And saves the garment
    Then the detail of the new garment is titled "Blue top"
    And the wardrobe contains one garment with that photo

  Scenario: category and color are required
    When the user saves the garment without choosing a category or a color
    Then the form asks to choose a category
    And the form asks to choose a color
    And nothing was added to the wardrobe

  Scenario: purchase information is saved with the garment
    When the user chooses the category "Top" and the color "Blue"
    And enters a price of 45000 "ARS" paid on "2026-03-15"
    And saves the garment
    Then the detail of the new garment is titled "Blue top"
    And the price "ARS 45,000.00" and the date "Mar 15, 2026" are shown

  Scenario: a purchase date in the future is rejected
    When the user chooses the category "Top" and the color "Blue"
    And enters a price of 45000 "ARS" paid tomorrow
    And saves the garment
    Then the form says the purchase date cannot be in the future
    And nothing was added to the wardrobe

  Scenario: a price without a date is incomplete
    When the user chooses the category "Top" and the color "Blue"
    And enters a price of 45000 "ARS" without a date
    And saves the garment
    Then the form asks for the purchase date

  Scenario: a garment can be saved without a photo
    When the user chooses the category "Footwear" and the color "White"
    And saves the garment
    Then the detail of the new garment is titled "White footwear"
