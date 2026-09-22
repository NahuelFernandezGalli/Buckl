Feature: Wardrobe filters and search

  As someone with a full closet
  I want to narrow the wardrobe by category, color, size or a word
  So that I find the garment I have in mind without scrolling

  Background:
    Given a wardrobe with a blue top sized M, a black bottom sized 32 and a red dress sized S
    And the blue top comes from the product "Oxford shirt" by "Uniqlo"

  Scenario Outline: filtering by category keeps only that category
    When the user filters the wardrobe by category "<category>"
    Then only the <expected> is listed

    Examples:
      | category | expected     |
      | Top      | blue top     |
      | Bottom   | black bottom |
      | Dress    | red dress    |

  Scenario: filtering by color keeps only that color
    When the user filters the wardrobe by color "Black"
    Then only the black bottom is listed

  Scenario: filtering by size keeps only that size
    When the user filters the wardrobe by size "S"
    Then only the red dress is listed

  Scenario: searching by a word matches the linked product
    When the user searches the wardrobe for "oxford"
    Then only the blue top is listed

  Scenario: filters live in the address so they survive a reload
    When the user opens the wardrobe at "/wardrobe?color=red"
    Then only the red dress is listed

  Scenario: clearing the filters shows the whole wardrobe again
    Given the wardrobe is filtered by category "Top"
    When the user clears the filters
    Then all three garments are listed

  Scenario: no garment matches the filters
    When the user filters the wardrobe by color "Pink"
    Then a message says no garment matches the filters
    And a way to show the whole wardrobe is offered

  Scenario: archived garments can be listed on request
    Given an archived grey top is also on record
    When the user asks to show archived garments
    Then only the grey top is listed
