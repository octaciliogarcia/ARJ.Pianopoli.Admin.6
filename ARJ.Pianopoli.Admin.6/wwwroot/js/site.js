var urlbase = "";
//var urlbase = "";
 
$(function () {


    $('.datepicker').mask("99/99/9999");
    $(".datepicker").datepicker({
    });

    $.datepicker.regional['pt-BR'] = {
        closeText: 'Fechar',
        prevText: '&#x3c;Anterior',
        nextText: 'Pr&oacute;ximo&#x3e;',
        currentText: 'Hoje',
        monthNames: ['Janeiro', 'Fevereiro', 'Mar&ccedil;o', 'Abril', 'Maio', 'Junho',
            'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'],
        monthNamesShort: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
            'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
        dayNames: ['Domingo', 'Segunda-feira', 'Ter&ccedil;a-feira', 'Quarta-feira', 'Quinta-feira', 'Sexta-feira', 'Sabado'],
        dayNamesShort: ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab'],
        dayNamesMin: ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab'],
        weekHeader: 'Sm',
        dateFormat: 'dd/mm/yy',
        firstDay: 0,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['pt-BR']);
 
    var options = {
        onKeyPress: function (cpf, ev, el, op) {
            var masks = ['000.000.000-000', '00.000.000/0000-00'];
            $('.cpfOuCnpj').mask((cpf.length > 14) ? masks[1] : masks[0], op);
        }
    }


    $('.cpfOuCnpj').mask('000.000.000-000', options);
    

    $('.telefone').focusout(function () {
        var phone, element;
        element = $(this);
        element.unmask();
        phone = element.val().replace(/\D/g, '');
        if (phone.length > 10) {
            element.mask("(99) 99999-9999");
        } else {
            element.mask("(99) 9999-99999");
        }
    }).trigger('focusout');

    $('.cpfcnpj').mask("99.999.999/9999-99");
    $('.cpf').mask("999.999.999-99");
    $('.cep').mask("99.999-999");
    $('.hora').mask("99:99");
    $('.tresNumeros').mask("999");
    $('.doisNumeros').mask("99");

    $('.numeros').mask("9999999999999");

    $('.periodo').mask("99/9999");
    $('.money').mask("999.999.999,99", { reverse: true });
});
function SwalPopUpConfirm(title, text, acao, codigo, DescricaoBotao ="Confirmar!") {
    swal({
        title: title,
        text: text,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        cancelButtonText: 'Cancelar',
        confirmButtonText: DescricaoBotao
    }).then((result) => {
        if (result.value) {
            acao(codigo);
        }
    });
}

function SwalPopUpConfirmFaturamento(title, text, acao, codigo, codigo2) {
    swal({
        title: title,
        text: text,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        cancelButtonText: 'Cancelar',
        confirmButtonText: 'Confirmar!'
    }).then((result) => {
        if (result.value) {
            acao(codigo, codigo2);
        }
    })

}



function SwalPopUp(title, mensagem, tipo) {

    swal({
        title: title,
        text: mensagem,
        type: 'success',
        showCancelButton: false,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Ok!'
    }).then((result) => {
        if (result.value) {
            RemoveAguarde();
        }
    })
}

function SwalPopUpErro(title, mensagem, tipo) {

    swal({
        title: title,
        text: mensagem,
        type: 'error',
        showCancelButton: false,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Ok!'
    }).then((result) => {
        if (result.value) {
            RemoveAguarde();
        }
    })
}

function Aguarde() {
    $.blockUI({
        message: "<h2>Aguarde...</h2>",
        css: {
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: .5,
            color: '#fff',
            cursor: 'default',
            theme: true,
            baseZ: 5000

        }
    });
    $.blockUI.defaults.baseZ = 4000;
}


function SwalPopUpSucess(title, mensagem, tipo) {

    swal({
        position: 'top-end',
        title: title,
        text: mensagem,
        type: 'success',
        showConfirmButton: false,
        timer: 2500
    })
}

function RemoveAguarde() {
    $.unblockUI();
}


function ValidaCpf() {
    var cpf = $("#Cpf").val();

    if (cpf != "") {
        if (TestaCPF(cpf) == false) {
            SwalPopUpErro("Erro", "CPF Inválido", "error");
            $(".cpf").val("");
            $("#Cpf").focus();

        }
    }

}

function TestaCPF(cpf) {
    var strCPF = cpf.replace(/[^0-9]/g, '');


    var Soma;
    var Resto;
    Soma = 0;
    if (strCPF == "00000000000") return false;
    if (strCPF == "11111111111") return false;
    if (strCPF == "22222222222") return false;
    if (strCPF == "33333333333") return false;
    if (strCPF == "44444444444") return false;
    if (strCPF == "55555555555") return false;
    if (strCPF == "66666666666") return false;
    if (strCPF == "77777777777") return false;
    if (strCPF == "77777777777") return false;

    for (i = 1; i <= 9; i++) Soma = Soma + parseInt(strCPF.substring(i - 1, i)) * (11 - i);
    Resto = (Soma * 10) % 11;

    if ((Resto == 10) || (Resto == 11)) Resto = 0;
    if (Resto != parseInt(strCPF.substring(9, 10))) return false;

    Soma = 0;
    for (i = 1; i <= 10; i++) Soma = Soma + parseInt(strCPF.substring(i - 1, i)) * (12 - i);
    Resto = (Soma * 10) % 11;

    if ((Resto == 10) || (Resto == 11)) Resto = 0;
    if (Resto != parseInt(strCPF.substring(10, 11))) return false;
    return true;
}

function MensagemSucesso(data) {
    $(".ConteudoMensagem").remove();
    $(".MensagemSucesso").empty();
    $("<div class='ConteudoMensagem'></div>").appendTo("body");
    if (data.isValid == false) {
        $('<div class="alert alert-danger mensagem">  <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a></div >').appendTo('.ConteudoMensagem');
        data.errors.forEach(function (entry) {
            $("<div class='row'>" + entry + "</div>").appendTo(".mensagem");
        });
    } else {

        if (!data.Result) {
            $("<div class=ConteudoMensagem></div>").appendTo("body");
            $('<div class="alert alert-danger alert-dismissible mensagem">   <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><br/></div >').appendTo('.ConteudoMensagem');
            $("<div class='row'>" + data.Message + "</div>").appendTo(".mensagem");

            return;
        } else if (data.Result == true) {
            $("<div class=ConteudoMensagem></div>").appendTo("body");
            $('<div class="alert alert-success alert-dismissible mensagem">    <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><br/></div >').appendTo('.ConteudoMensagem');
            $("<div class='row'>" + data.Message + "</div>").appendTo(".mensagem");

        }
    }
}

function MensagemJs(data) {
    $(".ConteudoMensagem").remove();
    $(".MensagemSucesso").empty();
    $("<div class='ConteudoMensagem'></div>").appendTo("body");
    if (data.isValid == false) {
        $('<div class="alert alert-danger mensagem">  <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a></div >').appendTo('.ConteudoMensagem');
        data.errors.forEach(function (entry) {
            $("<div class='row'>" + entry + "</div>").appendTo(".mensagem");
        });
    } else {

        if (!data.Result) {
            $("<div class=ConteudoMensagem></div>").appendTo("body");
            $('<div class="alert alert-danger alert-dismissible mensagem">   <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><br/></div >').appendTo('.ConteudoMensagem');
            $("<div class='row'>" + data.Message + "</div>").appendTo(".mensagem");

            return;
        } else if (data.Result == true) {
            $("<div class=ConteudoMensagem></div>").appendTo("body");
            $('<div class="alert alert-success alert-dismissible mensagem">    <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><br/></div >').appendTo('.ConteudoMensagem');
            $("<div class='row'>" + data.Message + "</div>").appendTo(".mensagem");

        }
    }
}


function Voltar() {
    $("#formulario").hide();
    $("#grid").show();
}


function Novo() {
    $("#formulario").show();
    $("#grid").hide();
}


function CalculaEntrada() {

    var valor = $("#Total").val();
    var number = valor.replace(".", "");
    number = number.replace(/,/, ".");
    var total = parseFloat(number).toFixed(2);
    var entradapermitida = total * 0.15;

    var entradadigitada = $("#Entrada").val();
    number = entradadigitada.replace(".", "").replace(/,/, ".");
    var valorentrada = parseFloat(number).toFixed(2);

    if (valorentrada < entradapermitida) {
        $("#Entrada-msg").attr("hidden", false);
    }
    else {
        $("#Entrada-msg").attr("hidden", true);
        $("#TipoPagto").attr("hidden", false);
        $("#Entrada").attr("disabled", true);

        if (valorentrada == total) {
            // mostra apenas a condição a vista e desabilita o componente
            $("#TipoPagamento").attr("disabled", true);
            $("#TipoPagamento").val(1);
        }
        else {
            $("#TipoPagamento").attr("disabled", true);
            $("#TipoPagamento").val(2);
        }

        var quadra = $("#Quadra").val();
        var lote = $("#Lote").val();

        $.ajax({
            cache: false,
            type: 'POST',
            data: {
                Quadra: quadra,
                Lote: lote,
                Entrada: $("#Entrada").val()
            },
            url: urlbase + '/Propostas/CalcularPrecos',
            success: function (data) {
                if (data.result == false) {
                    SwalPopUpErro("Atenção", data.message, "success");
                }
                else {
                    $("#SaldoPagar").val(data.retorno.saldoPagar);
                    $("#Entrada").val(data.retorno.entrada);
                    $("#btn-validar").attr("hidden", false);
                    $("#TipoPagamento").attr("readonly", true);

                    if (data.retorno.tipoPgtoPermitido == "1") {
                        $("#TipoPagamento").val(1)
                    }
                    else {
                        var conteudo = data.retorno.parcelas;
                        $("#TipoPagamento").val(2);
                        $("#Parcelamento").empty().append(conteudo);
                        $("#Parcelamento").attr("readonly", false);
                        document.getElementById('TipoPagamento').onchange(this.value);

                    }

                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
            }
        });

    }
}

