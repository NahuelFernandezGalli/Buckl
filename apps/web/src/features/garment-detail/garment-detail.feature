Feature: Garment detail

  As someone browsing my wardrobe
  I want to see everything I know about a garment
  So that I can recognize it, remember what it cost and find where it came from

  Scenario: opening a garment from the wardrobe shows its detail
    Given a wardrobe with a blue top sized M
    When the user opens the blue top from the wardrobe
    Then the detail is titled "Blue top"
    And the detail shows the photo, "Top", "Blue" and "M"

  Scenario: purchase information shows the price with its currency and the date
    Given a blue top bought for 45000 ARS on "2026-03-15"
    When the user opens the detail of the blue top
    Then the price "ARS 45,000.00" is shown
    And the purchase date "Mar 15, 2026" is shown

  Scenario: a garment without purchase information says so
    Given a blue top with no purchase information
    When the user opens the detail of the blue top
    Then the detail says there is no purchase information

  Scenario: a garment linked to a product shows the product
    Given a blue top that comes from the product "Oxford shirt" by "Uniqlo" at "https://example.com/oxford"
    When the user opens the detail of the blue top
    Then the product "Oxford shirt" by "Uniqlo" is shown
    And a link to the product page at "https://example.com/oxford" is offered

  Scenario: notes are shown
    Given a blue top with the note "Bought in Madrid"
    When the user opens the detail of the blue top
    Then the note "Bought in Madrid" is shown

  Scenario: an archived garment is marked as such
    Given an archived blue top
    When the user opens the detail of the blue top
    Then the detail is marked as archived

  Scenario: a garment that does not exist
    Given a wardrobe with no garments
    When the user opens the detail of a garment that does not exist
    Then a message says the garment is not in the wardrobe
    And a link back to the wardrobe is offered

  Scenario: a garment linked to a product that no longer exists
    Given a blue top linked to a product that no longer exists
    When the user opens the detail of the blue top
    Then the product section says the product is no longer available
