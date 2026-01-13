namespace Kcow.Infrastructure.Sql;

public static class StudentQueries
{
    public const string SelectColumns = @"
        id, reference, first_name, last_name, date_of_birth, gender, language,
        account_person_name, account_person_surname, account_person_id_number,
        account_person_cellphone, account_person_office, account_person_home, account_person_email,
        relation, mother_name, mother_surname, mother_office, mother_cell, mother_home, mother_email,
        father_name, father_surname, father_office, father_cell, father_home, father_email,
        address1, address2, postal_code, school_name, school_id, class_group_code, class_group_id,
        grade, teacher, attending_kcow_at, aftercare, extra, home_time, start_classes, terms, seat,
        truck, family, sequence, financial_code, charge, deposit, pay_date,
        tshirt_code, tshirt_money_1, tshirt_money_date_1, tshirt_received_1, tshirt_rec_date_1,
        receive_note_1, tshirt_size1, tshirt_color1, tshirt_design1,
        tshirt_size2, tshirt_money_2, tshirt_money_date_2, tshirt_received_2, tshirt_rec_date_2,
        receive_note_2, tshirt_color2, tshirt_design2,
        indicator1, indicator2, general_note, print_id_card, accept_terms_cond, status,
        sms_or_email, school_close, cnt, online_entry,
        created as LegacyCreated, submitted, updated as LegacyUpdated,
        book_email, report1_given_out, account_given_out, certificate_printed,
        report2_given_out, social, activity_report_given_out, photo_url, photo_updated,
        is_active, created_at, updated_at";
        
    public const string GetById = $"SELECT {SelectColumns} FROM students WHERE id = @Id";
    
    public const string GetAll = $"SELECT {SelectColumns} FROM students";
    
    public const string GetActive = $"SELECT {SelectColumns} FROM students WHERE is_active = 1";
}
