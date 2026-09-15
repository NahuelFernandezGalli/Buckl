Feature: Wardrobe list

  As someone who owns clothes
  I want to see everything in my wardrobe at a glance
  So that I can decide what to wear without emptying the closet

  Scenario: an empty wardrobe invites the first garment
    Given a wardrobe with no garments
    When the user opens the wardrobe
    Then an invitation to add the first garment is shown

  Scenario: garments are listed with their photo and category
    Given a wardrobe with a blue top and a black bottom
    When the user opens the wardrobe
    Then both garments are listed
    And each one shows its photo and its category

  Scenario: archived garments are not part of the wardrobe
    Given a wardrobe with an active blue top and an archived grey top
    When the user opens the wardrobe
    Then only the blue top is listed

  Scenario: each garment leads to its detail
    Given a wardrobe with a blue top
    When the user opens the wardrobe
    Then the blue top links to its detail
