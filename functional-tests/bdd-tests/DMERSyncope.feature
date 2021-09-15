Feature: DMERSyncope.feature
    As a Driver Medical Fitness SME
    I want to confirm the syncope business rules for a DMER

# non commercial
# not within 7 days > clean pass
Scenario: Non-Commercial Syncope Unexplained Single Not Within 7 Days > Clean Pass
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Cause Remains Unexplained radio button
    And I click on the Single Syncopal Event radio button
    And I click on No for Syncopal Event in the past 7 days
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# within 7 days > fail
Scenario: Non-Commercial Syncope Unexplained Single Less than 7 Days > Fail
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Cause Remains Unexplained radio button
    And I click on the Single Syncopal Event radio button
    And I click on Yes for Syncopal Event in the past 7 days    
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial 
# not within past 3 months > clean pass
Scenario: Non-Commercial Syncope Unexplained Recurrent Not Within 3 Months
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Cause Remains Unexplained radio button
    And I click on the Recurrent Syncopal Event radio button
    And I click on No for Syncopal Event in the past 3 months 
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial 
# within 3 months > fail
Scenario: Non-Commercial Syncope Unexplained Recurrent Within 3 Months
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Cause Remains Unexplained radio button
    And I click on the Recurrent Syncopal Event radio button
    And I click on Yes for Syncopal Event in the past 3 months 
    And I enter the recurrent unexplained past year syncope details
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# fail
Scenario: Non-Commercial Syncope Currently Untreated Single
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Currently Untreated radio button
    And I click on the Single Syncopal Event radio button
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# fail
Scenario: Non-Commercial Syncope Currently Untreated Recurrent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Currently Untreated radio button
    And I click on the Recurrent Syncopal Event radio button
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# within 7 days > fail
# commercial
# within 30 days > fail
Scenario: Non-Commercial Syncope Diagnosed, Treated Successfully, Single, Recent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Diagnosed, Treated Successfully radio button
    And I click on the Single Syncopal Event radio button
    And I click on Yes for Syncopal Event in the past 7 days    
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# within 7 days > fail
# commercial
# within 30 days > fail
Scenario: Non-Commercial Syncope Diagnosed, Treated Successfully, Single, Not Recent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Diagnosed, Treated Successfully radio button
    And I click on the Single Syncopal Event radio button
    And I click on No for Syncopal Event in the past 7 days    
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# not within 7 days > clean pass
# commercial
# not within 30 days > clean pass
Scenario: Non-Commercial Syncope Diagnosed, Treated Successfully, Recurrent, Not Recent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Diagnosed, Treated Successfully radio button
    And I click on the Recurrent Syncopal Event radio button
    And I click on No for Syncopal Event in the past 7 days    
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial
# not within 7 days > clean pass
# commercial
# not within 30 days > clean pass
Scenario: Non-Commercial Syncope Diagnosed, Treated Successfully, Recurrent, Recent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Diagnosed, Treated Successfully radio button
    And I click on the Recurrent Syncopal Event radio button
    And I click on Yes for Syncopal Event in the past 7 days    
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial > pass
# commercial > pass
Scenario: Non-Commercial Syncope Reversible, Treated Successfully, Single
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Reversible, Treated Successfully radio button
    And I click on the Single Syncopal Event radio button
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

# non commercial > pass
# commercial > pass
Scenario: Non-Commercial Syncope Reversible, Treated Successfully, Recurrent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    And I click on the Syncope checkbox
    And I expand the Syncope area
    And I click on the Reversible, Treated Successfully radio button
    And I click on the Recurrent Syncopal Event radio button
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

    Scenario: Non-Commercial Syncope Situational, Avoidable Trigger, Single, Past 7 Days No
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Non-Commercial Syncope Situational, Avoidable Trigger, Single, Past 7 Days Yes
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Non-Commercial Syncope Vasovagal, Single, Typical
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Non-Commercial Syncope Vasovagal, Recurrent, Atypical
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I do not select the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Unexplained Single No Repeat (2)
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial
    # not within 7 days > clean pass
    # within 7 days > fail
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Unexplained Recurrent Past Year (2)
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial 
    # not within past 3 months > clean pass
    # within 3 months > fail
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Currently Untreated Single (2)
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial
    # fail
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Currently Untreated Recurrent (2)
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    # And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial
    # fail
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Diagnosed, Treated Successfully, Single, Recent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial
    # within 7 days > fail
    # commercial
    # within 30 days > fail
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Diagnosed, Treated Successfully, Recurrent, Not Recent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial
    # not within 7 days > clean pass
    # commercial
    # not within 30 days > clean pass
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Reversible, Treated Successfully, Single
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial > pass
    # commercial > pass
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Reversible, Treated Successfully, Recurrent
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    # non commercial > pass
    # commercial > pass
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

    Scenario: Commercial Syncope Situational, Avoidable Trigger, Single, Past 7 Days No
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Situational, Avoidable Trigger, Single, Past 7 Days Yes
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Vasovagal, Single, Typical
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal

Scenario: Commercial Syncope Vasovagal, Recurrent, Atypical
    When I log in to the doctors' portal
    And I click on the DMER Forms tab
    And I click on the Case ID for 333
    And I refresh the page
    And I wait for the drivers licence field to have a value
    And I click on the Next button
    And the second page content is displayed
    And I click on the Next button
    And I enter the Uncorrected Binocular Vision as 20
    And I click on the Next button
    And I click on the Commercial DMER option
    # to be completed
    And I click on the Next button
    And I enter the medical opinion and confirmations
    And I click on the form submit button
    Then I log out of the portal